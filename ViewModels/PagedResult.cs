// Họ và tên: [SV2 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Lớp phân trang dùng chung (PhanTrang / PagedResult).

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.ViewModels;

public class PhanTrang
{
    public int Trang { get; set; } = 1;
    public int KichThuoc { get; set; } = 10;
    public int TongSo { get; set; }

    public int TongTrang => KichThuoc <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TongSo / (double)KichThuoc));
    public bool CoTrangTruoc => Trang > 1;
    public bool CoTrangSau => Trang < TongTrang;
}

public class PagedResult<T> : PhanTrang
{
    public List<T> Items { get; set; } = new();

    public PagedResult() { }

    public PagedResult(List<T> items, int trang, int kichThuoc, int tongSo)
    {
        Items = items;
        Trang = trang;
        KichThuoc = kichThuoc;
        TongSo = tongSo;
    }
}
