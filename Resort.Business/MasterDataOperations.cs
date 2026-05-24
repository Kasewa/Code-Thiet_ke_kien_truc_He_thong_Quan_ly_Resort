using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Model.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Resort.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var keys = await _unitOfWork.Repository<MasterDataKey>().GetAllAsync();
            return keys.OrderBy(k => k.Key).ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            var masterKey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(k => k.Key == key);
            if (masterKey is null) return new List<MasterDataValue>();

            var values = await _unitOfWork.Repository<MasterDataValue>()
                .FindAllAsync(v => v.MasterDataKeyId == masterKey.Id && v.IsActive);
            return values.OrderBy(v => v.DisplayOrder).ThenBy(v => v.Value).ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            key.Id = Guid.NewGuid().ToString();
            key.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateMasterKeyAsync(string id, MasterDataKey key)
        {
            var existing = await _unitOfWork.Repository<MasterDataKey>().GetByIdAsync(id);
            if (existing is null) return false;

            existing.Key = key.Key;
            existing.Description = key.Description;
            existing.IsActive = key.IsActive;
            existing.UpdatedBy = key.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<MasterDataKey>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            value.Id = Guid.NewGuid().ToString();
            value.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateMasterValueAsync(string id, MasterDataValue value)
        {
            var existing = await _unitOfWork.Repository<MasterDataValue>().GetByIdAsync(id);
            if (existing is null) return false;

            existing.Value = value.Value;
            existing.Description = value.Description;
            existing.DisplayOrder = value.DisplayOrder;
            existing.IsActive = value.IsActive;
            existing.UpdatedBy = value.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<MasterDataValue>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UploadBulkMasterDataAsync(List<MasterDataValue> values)
        {
            if (values is null || values.Count == 0) return false;

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value.MasterDataKeyId) || string.IsNullOrWhiteSpace(value.Value))
                    continue;

                var partitionKey = value.MasterDataKeyId.Trim();
                var masterKey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(k => k.Key == partitionKey);

                if (masterKey is null)
                {
                    masterKey = new MasterDataKey
                    {
                        Id = Guid.NewGuid().ToString(),
                        Key = partitionKey,
                        Description = partitionKey,
                        IsActive = true,
                        CreatedBy = value.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = value.UpdatedBy,
                        UpdatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<MasterDataKey>().AddAsync(masterKey);
                    await _unitOfWork.CommitAsync();
                }

                var existing = await _unitOfWork.Repository<MasterDataValue>()
                    .FindAsync(v => v.MasterDataKeyId == masterKey.Id && v.Value == value.Value);

                if (existing is null)
                {
                    value.Id = Guid.NewGuid().ToString();
                    value.MasterDataKeyId = masterKey.Id;
                    value.CreatedDate = DateTime.UtcNow;
                    await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                }
                else
                {
                    existing.Description = value.Description;
                    existing.DisplayOrder = value.DisplayOrder;
                    existing.IsActive = value.IsActive;
                    existing.UpdatedBy = value.UpdatedBy;
                    existing.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.Repository<MasterDataValue>().UpdateAsync(existing);
                }
            }

            return await _unitOfWork.CommitAsync() > 0;
        }
    }

    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly IMasterDataOperations _masterDataOperations;
        private const string CacheKey = "ResortMasterData";

        public MasterDataCacheOperations(IDistributedCache cache, IMasterDataOperations masterDataOperations)
        {
            _cache = cache;
            _masterDataOperations = masterDataOperations;
        }

        public async Task<Dictionary<string, List<string>>> GetMasterDataCacheAsync()
        {
            var cached = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(cached)
                    ?? new Dictionary<string, List<string>>();

            await CreateMasterDataCacheAsync();
            cached = await _cache.GetStringAsync(CacheKey);
            return string.IsNullOrEmpty(cached)
                ? new Dictionary<string, List<string>>()
                : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(cached)
                    ?? new Dictionary<string, List<string>>();
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var keys = await _masterDataOperations.GetAllMasterKeysAsync();
            var result = new Dictionary<string, List<string>>();

            foreach (var key in keys.Where(k => k.IsActive))
            {
                var values = await _masterDataOperations.GetAllMasterValuesByKeyAsync(key.Key);
                result[key.Key] = values.Select(v => v.Value).ToList();
            }

            await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
        }
    }
}
