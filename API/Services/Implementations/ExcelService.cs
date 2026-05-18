using API.Services.Interfaces;
using OfficeOpenXml;

namespace API.Services.Implementations
{
    public class ExcelService : IExcelService
    {
        private readonly IWebHostEnvironment _env;

        public ExcelService(IWebHostEnvironment env)
        {
            this._env = env;
        }

        public async Task<byte[]> ExportDeliveryNoteAsync(string req_nbr)
        {
            // 1. Lấy đường dẫn file template (Dùng ContentRootPath)
            // Đường dẫn sẽ là: [Thư mục Project]/Templates/DeliveryNoteTemplate.xlsx
            string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "DeliveryNoteTemplate.xlsx");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file template tại: {templatePath}");
            }

            return await Task.Run(() =>
            {
                // 2. Đọc file mẫu vào MemoryStream
                // Copy vào MemoryStream để không đụng chạm gì tới file gốc trên ổ cứng
                var templateBytes = File.ReadAllBytes(templatePath);

                using (var stream = new MemoryStream(templateBytes))
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên

                    // ============================================
                    // 3. FILL DỮ LIỆU TEST (Dòng 14 như yêu cầu)
                    // ============================================

                    int rowIndex = 14; // Dòng bắt đầu data theo template của bạn

                    // Cột A (STT) - index 1
                    ws.Cells[rowIndex, 1].Value = 1;

                    // Cột B (Tên hàng) - index 2
                    ws.Cells[rowIndex, 2].Value = "Cerrocast tube with acid etching 1st";

                    // Cột C (Mã) - index 3
                    ws.Cells[rowIndex, 3].Value = "MCN0197";

                    // Cột D (Đơn vị) - index 4
                    ws.Cells[rowIndex, 4].Value = "No requirement";

                    // Cột F (Qty Required) - index 6
                    ws.Cells[rowIndex, 6].Value = 400.00;

                    // Cột G (Qty Actual) - index 7 (Ví dụ test)
                    ws.Cells[rowIndex, 7].Value = 400.00;

                    // Cột H (Price) - index 8
                    ws.Cells[rowIndex, 8].Value = 10.5;

                    // Cột I (Amount) - index 9
                    // Tốt nhất là tính toán bằng C# rồi gán vào
                    ws.Cells[rowIndex, 9].Value = 400.00 * 10.5;

                    // Cột J (Remark) - index 10
                    ws.Cells[rowIndex, 10].Value = "Test Data";

                    // ============================================
                    // XỬ LÝ NẾU CÓ NHIỀU DÒNG (Logic mở rộng)
                    // ============================================
                    // Nếu data thật có nhiều hơn 1 dòng, bạn cần Insert thêm dòng
                    // để đẩy phần Footer (Tổng tiền, Chữ ký...) xuống dưới.
                    // int extraRows = totalItems - 1;
                    // if (extraRows > 0) ws.InsertRow(15, extraRows, 14);

                    // Tính lại công thức (nếu template có hàm SUM)
                    // package.Workbook.Calculate(); 

                    return package.GetAsByteArray();
                }
            });
        }
    }
}
