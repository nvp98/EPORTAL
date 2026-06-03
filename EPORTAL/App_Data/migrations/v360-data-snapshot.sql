-- ============================================================
-- V360 admin-config DATA SNAPSHOT (idempotent MERGE/UPDATE)
-- Generated: 2026-05-30 17:09:50 tu localhost,1433/EPORTAL
-- Re-apply: chay file nay sau khi DB schema da deploy (v360-all.sql).
-- ============================================================

-- ============================================================
-- 1 : dbo.V360_SceneCalibration
-- ============================================================
MERGE INTO dbo.V360_SceneCalibration AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d5fa-8969-b490' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.[Offset] = 0, T.HidePin = 0, T.CustomTitle = N'Cảng tổng hợp', T.UpdatedAt = '2026-05-29T11:47:33', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d5fa-8969-b490', 0, 0, N'Cảng tổng hợp', '2026-05-29T11:47:33', NULL);

MERGE INTO dbo.V360_SceneCalibration AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69e7-3378-b921-d795' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.[Offset] = NULL, T.HidePin = 0, T.CustomTitle = N'Main View', T.UpdatedAt = '2026-05-29T12:59:10', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69e7-3378-b921-d795', NULL, 0, N'Main View', '2026-05-29T12:59:10', NULL);

MERGE INTO dbo.V360_SceneCalibration AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d00e-a73d-f991' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.[Offset] = NULL, T.HidePin = 0, T.CustomTitle = N'Khách sạn The Harmonia', T.UpdatedAt = '2026-05-29T13:00:09', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d00e-a73d-f991', NULL, 0, N'Khách sạn The Harmonia', '2026-05-29T13:00:09', NULL);

MERGE INTO dbo.V360_SceneCalibration AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69d6-10c9-1a4a-1174' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.[Offset] = NULL, T.HidePin = 0, T.CustomTitle = N'Tòa nhà hành chính', T.UpdatedAt = '2026-05-29T13:47:53', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69d6-10c9-1a4a-1174', NULL, 0, N'Tòa nhà hành chính', '2026-05-29T13:47:53', NULL);

MERGE INTO dbo.V360_SceneCalibration AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0e2-ed49-6115' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.[Offset] = NULL, T.HidePin = 0, T.CustomTitle = N'Kho', T.UpdatedAt = '2026-05-29T13:50:30', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0e2-ed49-6115', NULL, 0, N'Kho', '2026-05-29T13:50:30', NULL);

PRINT '[1] dbo.V360_SceneCalibration : 5 rows merged';
GO

-- ============================================================
-- 2 : dbo.V360_FeaturedScene
-- ============================================================
MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d5fa-8969-b490' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 0, T.CustomTitle = NULL, T.ImagePath = N'~/Content/view360-featured/7Hknl/695cd5fa8969b490_639154724803204291.png', T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d5fa-8969-b490', 0, NULL, N'~/Content/view360-featured/7Hknl/695cd5fa8969b490_639154724803204291.png', '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69e7-3378-b921-d795' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 1, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69e7-3378-b921-d795', 1, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69e7-30bf-5b0d-d228' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 2, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69e7-30bf-5b0d-d228', 2, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'661f-3e11-2d5e-4153' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 3, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'661f-3e11-2d5e-4153', 3, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0df-4366-2190' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 4, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0df-4366-2190', 4, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0f2-3ac9-3896' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 5, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0f2-3ac9-3896', 5, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0e2-ed49-6115' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 6, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0e2-ed49-6115', 6, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0ee-7b13-5666' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 7, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0ee-7b13-5666', 7, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0e8-c538-8141' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 8, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0e8-c538-8141', 8, NULL, NULL, '2026-05-27T16:54:44', NULL);

MERGE INTO dbo.V360_FeaturedScene AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69e8-8a1b-8ef2-c157' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.DisplayOrder = 9, T.CustomTitle = NULL, T.ImagePath = NULL, T.UpdatedAt = '2026-05-27T16:54:44', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69e8-8a1b-8ef2-c157', 9, NULL, NULL, '2026-05-27T16:54:44', NULL);

PRINT '[2] dbo.V360_FeaturedScene : 10 rows merged';
GO

-- ============================================================
-- 3 : dbo.V360_TourConfig
-- ============================================================
MERGE INTO dbo.V360_TourConfig AS T
USING (SELECT N'7Hknl' AS CollectionId) AS S
   ON T.CollectionId = S.CollectionId
WHEN MATCHED THEN UPDATE SET T.PinSize = 10, T.PinColor = N'#f50000', T.SelectedColor = N'#2cc1e8', T.ConeColor = N'#00ccff', T.ConeFanDeg = 105, T.ConeRadius = 49, T.UpdatedAt = '2026-05-28T13:10:47', T.UpdatedBy = NULL
WHEN NOT MATCHED THEN INSERT (CollectionId, PinSize, PinColor, SelectedColor, ConeColor, ConeFanDeg, ConeRadius, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', 10, N'#f50000', N'#2cc1e8', N'#00ccff', 105, 49, '2026-05-28T13:10:47', NULL);

PRINT '[3] dbo.V360_TourConfig : 1 rows merged';
GO

-- ============================================================
-- 4 : dbo.V360_ChatbotTourInfo
-- ============================================================
MERGE INTO dbo.V360_ChatbotTourInfo AS T
USING (SELECT N'7Hknl' AS CollectionId) AS S
   ON T.CollectionId = S.CollectionId
WHEN MATCHED THEN UPDATE SET T.Overview = N'Công ty Cổ phần Thép Hòa Phát Dung Quất được thành lập tháng 2/2017, là chủ đầu tư xây dựng, vận hành Khu liên hợp sản xuất Gang thép Hòa Phát tại Khu kinh tế Dung Quất, tỉnh Quảng Ngãi.

Khu liên hợp sản xuất gang thép Hòa Phát Dung Quất có tổng vốn đầu tư 60.000 tỷ đồng, sản phẩm chủ yếu là thép xây dựng, thép cuộn chất lượng cao và sản phẩm dẹt là thép cuộn cán nóng. Hòa Phát áp dụng công nghệ lò cao khép kín tương tự mô hình đã triển khai thành công tại tỉnh Hải Dương, nhưng ưu việt hơn, thiết bị hiện đại hơn được nhập khẩu từ các nhà sản xuất hàng đầu thế giới. Đây là công nghệ tiên tiến, hiện đại, thân thiện với môi trường, sản xuất than coke bằng công nghệ dập coke khô, thu hồi hoàn toàn nhiệt và khí thải, tận dụng triệt để sản phẩm phụ để phát điện, phục vụ trở lại sản xuất. Toàn bộ nguồn nước sản xuất cũng được sử dụng tuần hoàn, không xả ra môi trường.

Khu liên hợp gang thép Hòa Phát Dung Quất sẽ bao gồm hệ thống cảng biển nước sâu cho phép tàu 200.000 tấn cập bến, dễ dàng vận chuyển nguyên vật liệu đầu vào và sản phẩm đầu ra đi các thị trường trong và ngoài nước.

Hiện tại, Công ty đang triển khai dự án Khu liên hợp sản xuất gang thép Hòa Phát Dung Quất 2 với vốn đầu tư 85.000 tỷ đồng. Đây là dự án chiến lược quan trọng, đóng góp tích cực vào tăng trưởng giá trị sản xuất công nghiệp và GDP cả nước, đồng thời nâng cao vị thế nhà sản xuất thép hàng đầu khu vực Đông Nam Á cho Tập đoàn Hòa Phát.', T.SystemPrompt = NULL, T.IsEnabled = 1, T.UpdatedAt = '2026-05-29T11:47:59', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, Overview, SystemPrompt, IsEnabled, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'Công ty Cổ phần Thép Hòa Phát Dung Quất được thành lập tháng 2/2017, là chủ đầu tư xây dựng, vận hành Khu liên hợp sản xuất Gang thép Hòa Phát tại Khu kinh tế Dung Quất, tỉnh Quảng Ngãi.

Khu liên hợp sản xuất gang thép Hòa Phát Dung Quất có tổng vốn đầu tư 60.000 tỷ đồng, sản phẩm chủ yếu là thép xây dựng, thép cuộn chất lượng cao và sản phẩm dẹt là thép cuộn cán nóng. Hòa Phát áp dụng công nghệ lò cao khép kín tương tự mô hình đã triển khai thành công tại tỉnh Hải Dương, nhưng ưu việt hơn, thiết bị hiện đại hơn được nhập khẩu từ các nhà sản xuất hàng đầu thế giới. Đây là công nghệ tiên tiến, hiện đại, thân thiện với môi trường, sản xuất than coke bằng công nghệ dập coke khô, thu hồi hoàn toàn nhiệt và khí thải, tận dụng triệt để sản phẩm phụ để phát điện, phục vụ trở lại sản xuất. Toàn bộ nguồn nước sản xuất cũng được sử dụng tuần hoàn, không xả ra môi trường.

Khu liên hợp gang thép Hòa Phát Dung Quất sẽ bao gồm hệ thống cảng biển nước sâu cho phép tàu 200.000 tấn cập bến, dễ dàng vận chuyển nguyên vật liệu đầu vào và sản phẩm đầu ra đi các thị trường trong và ngoài nước.

Hiện tại, Công ty đang triển khai dự án Khu liên hợp sản xuất gang thép Hòa Phát Dung Quất 2 với vốn đầu tư 85.000 tỷ đồng. Đây là dự án chiến lược quan trọng, đóng góp tích cực vào tăng trưởng giá trị sản xuất công nghiệp và GDP cả nước, đồng thời nâng cao vị thế nhà sản xuất thép hàng đầu khu vực Đông Nam Á cho Tập đoàn Hòa Phát.', NULL, 1, '2026-05-29T11:47:59', 16090);

PRINT '[4] dbo.V360_ChatbotTourInfo : 1 rows merged';
GO

-- ============================================================
-- 5 : dbo.V360_ChatbotSceneInfo
-- ============================================================
MERGE INTO dbo.V360_ChatbotSceneInfo AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69e7-3378-b921-d795' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.ShortIntro = N'View chính', T.DetailContent = N'View tổng quan HPDQ', T.UpdatedAt = '2026-05-29T12:59:10', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69e7-3378-b921-d795', N'View chính', N'View tổng quan HPDQ', '2026-05-29T12:59:10', 16090);

MERGE INTO dbo.V360_ChatbotSceneInfo AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d5fa-8969-b490' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.ShortIntro = N'Cảng Hòa Phát Dung Quất (HPDQ) là cảng biển quốc tế nước sâu quy mô lớn tại Khu kinh tế Dung Quất (Quảng Ngãi)', T.DetailContent = N'Cảng Tổng Hợp Hòa Phát được thành lập tháng 05/2019, được chính phủ Việt Nam cho phép đầu tư xây dựng vào năm 2019 với tổng vốn đầu tư hơn 3.774 tỷ đồng.
Cảng Tổng Hợp Hòa Phát hoạt động và phát triển theo tiêu chuẩn một cảng biển Quốc tế. Với tổng diện tích gần 500.000 m2, Cảng Tổng Hợp Hòa Phát đã xây dựng một hệ thống kho bãi đạt tiêu chuẩn quốc tế đáp ứng cho việc lưu trữ cũng như tạm nhập tái xuất các loại hàng hóa, container, sắt thép, các loại thiết bị, hàng hóa siêu trường siêu trọng.', T.UpdatedAt = '2026-05-29T11:47:55', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d5fa-8969-b490', N'Cảng Hòa Phát Dung Quất (HPDQ) là cảng biển quốc tế nước sâu quy mô lớn tại Khu kinh tế Dung Quất (Quảng Ngãi)', N'Cảng Tổng Hợp Hòa Phát được thành lập tháng 05/2019, được chính phủ Việt Nam cho phép đầu tư xây dựng vào năm 2019 với tổng vốn đầu tư hơn 3.774 tỷ đồng.
Cảng Tổng Hợp Hòa Phát hoạt động và phát triển theo tiêu chuẩn một cảng biển Quốc tế. Với tổng diện tích gần 500.000 m2, Cảng Tổng Hợp Hòa Phát đã xây dựng một hệ thống kho bãi đạt tiêu chuẩn quốc tế đáp ứng cho việc lưu trữ cũng như tạm nhập tái xuất các loại hàng hóa, container, sắt thép, các loại thiết bị, hàng hóa siêu trường siêu trọng.', '2026-05-29T11:47:55', 16090);

MERGE INTO dbo.V360_ChatbotSceneInfo AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d00e-a73d-f991' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.ShortIntro = N'Khách sạn The Harmonia', T.DetailContent = N'Khách sạn The Harmonia (thuộc Tập đoàn Hòa Phát) là khách sạn 4 sao nằm tại Khu Kinh tế Dung Quất, xã Vạn Tường, huyện Bình Sơn, tỉnh Quảng Ngãi. Nơi đây cung cấp không gian nghỉ dưỡng yên bình với 142 phòng nghỉ, hồ bơi, phòng gym, sân thể thao và nhà hàng Á - Âu', T.UpdatedAt = '2026-05-29T13:00:09', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d00e-a73d-f991', N'Khách sạn The Harmonia', N'Khách sạn The Harmonia (thuộc Tập đoàn Hòa Phát) là khách sạn 4 sao nằm tại Khu Kinh tế Dung Quất, xã Vạn Tường, huyện Bình Sơn, tỉnh Quảng Ngãi. Nơi đây cung cấp không gian nghỉ dưỡng yên bình với 142 phòng nghỉ, hồ bơi, phòng gym, sân thể thao và nhà hàng Á - Âu', '2026-05-29T13:00:09', 16090);

MERGE INTO dbo.V360_ChatbotSceneInfo AS T
USING (SELECT N'7Hknl' AS CollectionId, N'69d6-10c9-1a4a-1174' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.ShortIntro = N'Tòa nhà hành chính', T.DetailContent = N'Tòa nhà hành chính Thép Hòa Phát Dung Quất (HPDQ) quy tụ các phòng ban đầu não của Khu liên hợp Công ty CP Thép Hòa Phát Dung Quất. Nơi đây chịu trách nhiệm quản lý vận hành, chiến lược và các nghiệp vụ hỗ trợ cho toàn bộ nhà máy tại Khu kinh tế Dung Quất, Quảng Ngãi Thép Hòa Phát Dung Quất.Các bộ phận chính làm việc tại tòa nhà hành chính bao gồm:Ban Giám đốc: Điều hành và quản lý chung mọi hoạt động sản xuất kinh doanh của công ty.Phòng Hành chính - Nhân sự (HR): Tuyển dụng, đào tạo, quản lý chế độ phúc lợi, nhà ở, xe đưa đón và chăm lo đời sống CBNV Nhân viên Nhân sự - Thép Hòa Phát Dung Quất.Phòng Kế toán - Tài chính: Quản lý dòng tiền, lương thưởng, thanh quyết toán và ngân sách.Phòng Kế hoạch - Vật tư: Phân tích nhu cầu sản xuất, mua sắm vật tư, thiết bị và quản lý chuỗi cung ứng.Phòng Kinh doanh: Tiếp nhận đơn hàng, điều phối tiêu thụ và chăm sóc khách hàng trong và ngoài nước.Phòng Quản lý Chất lượng (QC): Giám sát tiêu chuẩn chất lượng phôi thép, thép cuộn, thép xây dựng.Phòng Pháp chế: Xử lý các vấn đề liên quan đến văn bản, hợp đồng và tuân thủ pháp luật.Phòng CNTT (IT): Vận hành, bảo mật và hỗ trợ hệ thống quản trị nguồn lực doanh nghiệp (ERP - SAP S/4HANA) cũng như hệ thống quản lý năng lượng (EMS) Khu liên hợp gang thép Hòa Phát Dung Quất chính thức vận ....Lưu ý: Các khối kỹ thuật, sản xuất trực tiếp (lò cao, luyện thép, cán thép, cảng biển) làm việc chủ yếu tại các phân xưởng và trạm điều hành sản xuất thay vì khối tòa nhà hành chính.Bạn đang quan tâm đến thủ tục, cơ hội ứng tuyển hay có nhu cầu liên hệ làm việc với phòng ban cụ thể nào tại Hòa Phát Dung Quất? Nếu cần, bạn có thể tham khảo trực tiếp trên Cổng thông tin tuyển dụng Hòa Phát Dung Quất để được hỗ trợ.', T.UpdatedAt = '2026-05-29T13:47:53', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'69d6-10c9-1a4a-1174', N'Tòa nhà hành chính', N'Tòa nhà hành chính Thép Hòa Phát Dung Quất (HPDQ) quy tụ các phòng ban đầu não của Khu liên hợp Công ty CP Thép Hòa Phát Dung Quất. Nơi đây chịu trách nhiệm quản lý vận hành, chiến lược và các nghiệp vụ hỗ trợ cho toàn bộ nhà máy tại Khu kinh tế Dung Quất, Quảng Ngãi Thép Hòa Phát Dung Quất.Các bộ phận chính làm việc tại tòa nhà hành chính bao gồm:Ban Giám đốc: Điều hành và quản lý chung mọi hoạt động sản xuất kinh doanh của công ty.Phòng Hành chính - Nhân sự (HR): Tuyển dụng, đào tạo, quản lý chế độ phúc lợi, nhà ở, xe đưa đón và chăm lo đời sống CBNV Nhân viên Nhân sự - Thép Hòa Phát Dung Quất.Phòng Kế toán - Tài chính: Quản lý dòng tiền, lương thưởng, thanh quyết toán và ngân sách.Phòng Kế hoạch - Vật tư: Phân tích nhu cầu sản xuất, mua sắm vật tư, thiết bị và quản lý chuỗi cung ứng.Phòng Kinh doanh: Tiếp nhận đơn hàng, điều phối tiêu thụ và chăm sóc khách hàng trong và ngoài nước.Phòng Quản lý Chất lượng (QC): Giám sát tiêu chuẩn chất lượng phôi thép, thép cuộn, thép xây dựng.Phòng Pháp chế: Xử lý các vấn đề liên quan đến văn bản, hợp đồng và tuân thủ pháp luật.Phòng CNTT (IT): Vận hành, bảo mật và hỗ trợ hệ thống quản trị nguồn lực doanh nghiệp (ERP - SAP S/4HANA) cũng như hệ thống quản lý năng lượng (EMS) Khu liên hợp gang thép Hòa Phát Dung Quất chính thức vận ....Lưu ý: Các khối kỹ thuật, sản xuất trực tiếp (lò cao, luyện thép, cán thép, cảng biển) làm việc chủ yếu tại các phân xưởng và trạm điều hành sản xuất thay vì khối tòa nhà hành chính.Bạn đang quan tâm đến thủ tục, cơ hội ứng tuyển hay có nhu cầu liên hệ làm việc với phòng ban cụ thể nào tại Hòa Phát Dung Quất? Nếu cần, bạn có thể tham khảo trực tiếp trên Cổng thông tin tuyển dụng Hòa Phát Dung Quất để được hỗ trợ.', '2026-05-29T13:47:53', 16090);

MERGE INTO dbo.V360_ChatbotSceneInfo AS T
USING (SELECT N'7Hknl' AS CollectionId, N'695c-d0e2-ed49-6115' AS SceneUuid) AS S
   ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET T.ShortIntro = NULL, T.DetailContent = N'Kho HPDQ (Kho Công ty Cổ phần Thép Hòa Phát Dung Quất) là hệ thống nhà kho lưu trữ vật tư, thiết bị và thành phẩm thuộc Khu liên hợp sản xuất gang thép Hòa Phát. Kho đóng vai trò thiết yếu trong việc đảm bảo chuỗi cung ứng vật liệu cho toàn bộ dự án tại Khu kinh tế Dung Quất, tỉnh Quảng Ngãi.Thông tin chính về bộ phận Kho tại Hòa Phát Dung Quất:Vị trí: Khu kinh tế Dung Quất, xã Bình Đông (và xã Vạn Tường), huyện Bình Sơn, tỉnh Quảng Ngãi.Chức năng: Quản lý xuất/nhập vật tư, bảo quản hàng hóa, và điều phối nguồn nguyên vật liệu phục vụ hoạt động sản xuất liên tục của tập đoàn.Đào tạo nghiệp vụ: Công ty thường xuyên tổ chức các khóa đào tạo chuyên sâu về "Nghiệp vụ quản lý kho" cho hàng trăm học viên là nhân sự quản lý và nhân viên kho để tối ưu hóa hiệu suất.', T.UpdatedAt = '2026-05-29T13:50:30', T.UpdatedBy = 16090
WHEN NOT MATCHED THEN INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy) VALUES (N'7Hknl', N'695c-d0e2-ed49-6115', NULL, N'Kho HPDQ (Kho Công ty Cổ phần Thép Hòa Phát Dung Quất) là hệ thống nhà kho lưu trữ vật tư, thiết bị và thành phẩm thuộc Khu liên hợp sản xuất gang thép Hòa Phát. Kho đóng vai trò thiết yếu trong việc đảm bảo chuỗi cung ứng vật liệu cho toàn bộ dự án tại Khu kinh tế Dung Quất, tỉnh Quảng Ngãi.Thông tin chính về bộ phận Kho tại Hòa Phát Dung Quất:Vị trí: Khu kinh tế Dung Quất, xã Bình Đông (và xã Vạn Tường), huyện Bình Sơn, tỉnh Quảng Ngãi.Chức năng: Quản lý xuất/nhập vật tư, bảo quản hàng hóa, và điều phối nguồn nguyên vật liệu phục vụ hoạt động sản xuất liên tục của tập đoàn.Đào tạo nghiệp vụ: Công ty thường xuyên tổ chức các khóa đào tạo chuyên sâu về "Nghiệp vụ quản lý kho" cho hàng trăm học viên là nhân sự quản lý và nhân viên kho để tối ưu hóa hiệu suất.', '2026-05-29T13:50:30', 16090);

PRINT '[5] dbo.V360_ChatbotSceneInfo : 5 rows merged';
GO

-- ============================================================
-- 6 : dbo.ProjectsGroup (SortOrder + ParentIDGroup, key=GroupName)
-- ============================================================
-- 3D-VR 360
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'3D-VR 360')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'3D-VR 360'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'3D-VR 360', NULL, 0);

-- Cá»•ng-Camera
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Cá»•ng-Camera')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Cá»•ng-Camera'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Cá»•ng-Camera', NULL, 0);

-- DA. ThÃ©pRay
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'DA. ThÃ©pRay')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'DA. ThÃ©pRay'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'DA. ThÃ©pRay', NULL, 0);

-- HPDQ1
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ1')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'HPDQ1'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'HPDQ1', NULL, 0);

-- HPDQ2
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ2')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'HPDQ2'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'HPDQ2', NULL, 0);

-- PhÃº YÃªn
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'PhÃº YÃªn')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'PhÃº YÃªn'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'PhÃº YÃªn', NULL, 0);

-- HPDQ 2
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 1
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'HPDQ 2'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'HPDQ 2', NULL, 1);

-- HPDQ 1
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 1')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 2
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'HPDQ 1'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'HPDQ 1', NULL, 2);

-- Phú Yên
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Phú Yên')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 3
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Phú Yên'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Phú Yên', NULL, 3);

-- Cổng-Camera
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Cổng-Camera')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 4
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Cổng-Camera'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Cổng-Camera', NULL, 4);

-- 3D-VR360
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'3D-VR360')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 5
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'3D-VR360'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'3D-VR360', NULL, 5);

-- DA.ThepRay
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'DA.ThepRay')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 6
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'DA.ThepRay'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'DA.ThepRay', NULL, 6);

-- CTH
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'CTH')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 7
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'CTH'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'CTH', NULL, 7);

-- KHO
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'KHO')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 8
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'KHO'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'KHO', NULL, 8);

-- KSMB
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'KSMB')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = NULL, SortOrder = 9
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'KSMB'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'KSMB', NULL, 9);

-- Cáº£ng 11 KÃ©o dÃ i
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Cáº£ng 11 KÃ©o dÃ i')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 1')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Cáº£ng 11 KÃ©o dÃ i'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Cáº£ng 11 KÃ©o dÃ i', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 1')) AND ParentIDGroup IS NULL), 0);

-- Dá»± Ã¡n ÄÃºc 4
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dá»± Ã¡n ÄÃºc 4')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dá»± Ã¡n ÄÃºc 4'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dá»± Ã¡n ÄÃºc 4', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 0);

-- Dá»± Ã¡n CÃ¡n 4
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dá»± Ã¡n CÃ¡n 4')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dá»± Ã¡n CÃ¡n 4'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dá»± Ã¡n CÃ¡n 4', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 0);

-- Dá»± Ã¡n Kho than kÃ©o dÃ i
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dá»± Ã¡n Kho than kÃ©o dÃ i')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dá»± Ã¡n Kho than kÃ©o dÃ i'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dá»± Ã¡n Kho than kÃ©o dÃ i', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 0);

-- Dá»± Ã¡n LÃ² Quay ÄÃ¡y
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dá»± Ã¡n LÃ² Quay ÄÃ¡y')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dá»± Ã¡n LÃ² Quay ÄÃ¡y'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dá»± Ã¡n LÃ² Quay ÄÃ¡y', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 0);

-- NM.LG2
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'NM.LG2')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 0
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'NM.LG2'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'NM.LG2', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 0);

-- 2D
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'2D ')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 1
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'2D '));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'2D ', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 1);

-- Cảng 11 Kéo dài
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Cảng 11 Kéo dài')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 1')) AND ParentIDGroup IS NULL), SortOrder = 1
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Cảng 11 Kéo dài'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Cảng 11 Kéo dài', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 1')) AND ParentIDGroup IS NULL), 1);

-- Dự án Lò Quay Đáy
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dự án Lò Quay Đáy')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 2
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dự án Lò Quay Đáy'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dự án Lò Quay Đáy', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 2);

-- Dự án Cán 4
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dự án Cán 4')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 3
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dự án Cán 4'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dự án Cán 4', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 3);

-- Dự án Đúc 4
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dự án Đúc 4')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 4
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dự án Đúc 4'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dự án Đúc 4', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 4);

-- Dự án Kho than kéo dài
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'Dự án Kho than kéo dài')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 5
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'Dự án Kho than kéo dài'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'Dự án Kho than kéo dài', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 5);

-- NM.LG 2
IF EXISTS (SELECT 1 FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'NM.LG 2')))
    UPDATE dbo.ProjectsGroup
       SET ParentIDGroup = (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), SortOrder = 6
     WHERE LTRIM(RTRIM(GroupName)) = LTRIM(RTRIM(N'NM.LG 2'));
ELSE
    INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder)
    VALUES (N'NM.LG 2', (SELECT TOP 1 IDGroup FROM dbo.ProjectsGroup WHERE LTRIM(RTRIM(GroupName))=LTRIM(RTRIM(N'HPDQ 2')) AND ParentIDGroup IS NULL), 6);

PRINT '[6] dbo.ProjectsGroup : 28 rows merged (key=GroupName)';
GO

PRINT '';
PRINT '=== DATA SNAPSHOT APPLIED ===';
GO
