-- ================================================
-- SCRIPT RESET VÀ GENERATE DATA (5 CATS & 50 PRODS)
-- ================================================

USE [POSDB];
GO

-- 1. XÓA DỮ LIỆU CŨ
DELETE FROM [dbo].[Products];
DELETE FROM [dbo].[Categories];

-- 2. RESET IDENTITY VỀ 0
DBCC CHECKIDENT ('[dbo].[Categories]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Products]', RESEED, 0);

DECLARE @CatIds TABLE (Id INT, Name NVARCHAR(200));

-- 3. CHÈN 5 CATEGORIES
INSERT INTO [dbo].[Categories] ([Name], [Description])
OUTPUT INSERTED.Id, INSERTED.Name INTO @CatIds
VALUES 
(N'Thực phẩm khô', N'Mỳ tôm, gạo, gia vị và đồ đóng hộp'),
(N'Đồ uống', N'Nước giải khát, bia, trà và cà phê'),
(N'Bánh kẹo & Snack', N'Bánh quy, kẹo dẻo và đồ ăn vặt'),
(N'Hóa mỹ phẩm', N'Dầu gội, bột giặt và đồ dùng cá nhân'),
(N'Đồ dùng gia đình', N'Dụng cụ nhà bếp, điện gia dụng và vệ sinh');

-- 4. CHÈN 50 PRODUCTS (Chia đều 10 sp mỗi Cat)
INSERT INTO [dbo].[Products] ([CategoryId], [Barcode], [ProductName], [Price], [CostPrice], [StockQuantity], [IsActive])
VALUES
-- Nhóm 1: Thực phẩm khô
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893001', N'Mỳ Hảo Hảo Tôm Chua Cay', 4500, 3200, 500, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893002', N'Gạo ST25 Túi 5kg', 185000, 155000, 40, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893003', N'Nước Mắm Nam Ngư 750ml', 42000, 33000, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893004', N'Dầu Ăn Simply 1L', 54000, 42000, 80, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893005', N'Hạt Nêm Knorr 400g', 38000, 29000, 120, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893006', N'Tương Ớt Cholimex', 14000, 9500, 200, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893007', N'Mỳ Trộn Indomie', 6000, 4000, 300, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893008', N'Bún Tươi Sấy Khô', 18000, 13000, 60, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893009', N'Đường Biên Hòa 1kg', 26000, 21000, 150, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Thực phẩm khô'), '893010', N'Pate Gan Hạ Long', 22000, 16000, 45, 1),

-- Nhóm 2: Đồ uống
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893011', N'Coca Cola Lon 330ml', 10000, 7500, 240, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893012', N'Nước Suối Lavie 500ml', 6000, 3800, 600, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893013', N'Bia Heineken Lon', 21000, 17500, 120, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893014', N'Trà Xanh C2', 9000, 6200, 180, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893015', N'Cà phê Phố Sữa Đá', 45000, 32000, 90, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893016', N'Nước Cam ép Twister', 12000, 8500, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893017', N'Sữa Đậu Nành Fami', 7000, 4800, 300, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893018', N'Nước Tăng Lực Redbull', 15000, 11500, 150, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893019', N'Trà Ô Long Tea Plus', 11000, 8200, 200, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ uống'), '893020', N'Sữa Tươi TH True Milk', 35000, 28000, 120, 1),

-- Nhóm 3: Bánh kẹo & Snack
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893021', N'Snack Khoai Tây Lays', 12000, 8500, 150, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893022', N'Bánh Quy Oreo', 16000, 11500, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893023', N'Kẹo Dẻo Haribo', 28000, 20000, 80, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893024', N'Bánh Chocopie Hộp 12', 55000, 42000, 40, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893025', N'Socola Kitkat 4F', 14000, 10000, 120, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893026', N'Bánh Gạo One One', 24000, 18000, 70, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893027', N'Kẹo Cao Su Doublemint', 6000, 4000, 300, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893028', N'Snack Poca Mực', 12000, 8800, 140, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893029', N'Bánh Que Pocky', 20000, 15000, 90, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Bánh kẹo & Snack'), '893030', N'Hạt Điều Rang Muối', 125000, 95000, 25, 1),

-- Nhóm 4: Hóa mỹ phẩm
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893031', N'Dầu Gội Clear 650g', 155000, 120000, 30, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893032', N'Sữa Tắm Lifebuoy 850g', 140000, 110000, 35, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893033', N'Kem Đánh Răng PS', 35000, 26000, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893034', N'Bột Giặt Omo 3kg', 175000, 145000, 20, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893035', N'Nước Rửa Chén Sunlight', 28000, 21000, 60, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893036', N'Nước Lau Sàn Gift', 32000, 24000, 50, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893037', N'Xà Bông Cục Lux', 12000, 8000, 200, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893038', N'Khăn Giấy Ướt Bobby', 34000, 25000, 80, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893039', N'Dầu Xả Dove 320g', 85000, 65000, 40, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Hóa mỹ phẩm'), '893040', N'Nước Tẩy Bồn Cầu Vim', 36000, 27000, 45, 1),

-- Nhóm 5: Đồ dùng gia đình (Thay thế Ghibli)
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893041', N'Ấm Đun Siêu Tốc 1.8L', 250000, 185000, 15, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893042', N'Chảo Chống Dính 24cm', 195000, 145000, 12, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893043', N'Bộ Đũa Gỗ Mun (10 đôi)', 65000, 40000, 30, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893044', N'Thớt Gỗ Nghiến', 125000, 85000, 8, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893045', N'Ổ Cắm Điện Đa Năng', 85000, 60000, 25, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893046', N'Bóng Đèn Led 12W', 45000, 32000, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893047', N'Hộp Nhựa Duy Tân 5L', 35000, 24000, 50, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893048', N'Khăn Mặt Cotton', 25000, 15000, 100, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893049', N'Chổi Cỏ Quét Nhà', 35000, 20000, 40, 1),
((SELECT Id FROM @CatIds WHERE Name = N'Đồ dùng gia đình'), '893050', N'Nước Xịt Côn Trùng', 55000, 42000, 60, 1);

GO

-- Kiểm tra kết quả
SELECT c.Name as Category, COUNT(p.Id) as ProductCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Name;