// Resort Management System — site.js

// Auto-dismiss alerts after 4 seconds
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert.alert-success, .alert.alert-danger').forEach(function (el) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert?.close();
        }, 4000);
    });
});

// Confirm delete
function confirmDelete(form, message) {
    if (confirm(message || 'Bạn có chắc chắn muốn xóa không?')) {
        form.submit();
    }
}
