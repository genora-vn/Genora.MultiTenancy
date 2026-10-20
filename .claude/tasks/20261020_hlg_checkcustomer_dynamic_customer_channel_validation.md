[HLG] Kiểm tra API CheckCustomer (auth/{phone}) của HoaLinhGamificationController bổ sung thêm ràng buộc điều kiện khi gọi API lấy thông tin khách hàng từ hệ thống Hoa Linh DMS (_dms.GetCustomerByPhoneAsync(pharmaPhone)).

BỐI CẢNH
- Front end nhập số điện thoại để kiểm tra có phải số điện thoại Chủ nhà thuốc tồn tại bên hệ thống Hoa Linh DMS không. Nhấn kiểm tra và gọi API auth/{phone}.
- Backend đang trả ra hết danh sách mà bên hệ thống Hoa Linh DMS trả về. Điều này dẫn đến khách hàng có trong hệ thống của Hoa Linh DMS nhưng không thuộc Loại khách hành là pharmacy (Nhà thuốc / Dược) mà là thuộc loại khách hàng khác như retailer - Chủ cửa hàng, siêu thị, tạp hóa, consumer - Khách hàng tiêu dùng (Tự do).

YÊU CẦU
- Kiểm tra danh sách chi nhánh mà hệ thống Hoa Linh DMS trả về và lọc lấy ra danh sách chi nhánh có "cust_channel": "OTC" và trả về dữ liệu như hiện tại trong đó có danh sách chi nhánh (branches).
- Nếu không có bản ghi nào có "cust_channel": "OTC" thì trả về thông báo lỗi: "Cảm ơn Quý khách quan tâm chương trình. Rất tiếc, chương trình hiện tại đang áp dụng cho Khách hàng thuộc Kênh Nhà Thuốc".

LƯU Ý ĐẶC BIỆT:
- Tạm thời đang cấu hình chương trình chỉ áp dụng cho pharmacy - Kênh Nhà thuốc / Dược (OTC) sau này sẽ mở thêm chương trình cho các kênh khác như consumer, retailer.
- Hãy thêm cấu hình động để có thể áp dụng động cho các Loại khách hàng. Hiện tại là chỉ áp dụng cho pharmacy - Kênh Nhà thuốc / Dược (OTC).