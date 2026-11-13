//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2 - Tự động lọc khi đổi giá trị dropdown / ô ngày (không cần bấm nút "Lọc").
// Dùng: <form data-auto-filter> ... </form>. Form là method GET và không có input pageIndex
// nên mỗi lần đổi bộ lọc sẽ tự quay về trang 1; các điều kiện khác vẫn được giữ nguyên.
(() => {
    document.querySelectorAll('form[data-auto-filter]').forEach((form) => {
        form.addEventListener('change', (event) => {
            const el = event.target;
            if (!(el instanceof HTMLElement)) return;
            // Ô tìm kiếm văn bản vẫn dùng nút "Tìm kiếm" hoặc phím Enter.
            if (el.matches('select, input[type="date"]')) form.submit();
        });
    });
})();
