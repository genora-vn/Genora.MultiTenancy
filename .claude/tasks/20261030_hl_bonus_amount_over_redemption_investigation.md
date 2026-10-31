[HL - Hoa Linh Sale] Kiểm tra vấn đề số lượng tiền thưởng khách đổi được ít nhưng lại đổi được nhiều quà (vượt hạn mức cho phép trường BonusAmount).

BỐI CẢNH
1. Hệ thống ABP framework multi tenant Genora:
- Trên môi trường production có các khác hàng chỉ đổi tiền thưởng với bảng HL.AppHlGiftExchanges có dữ liệu như sau:
Id	TenantId	CustomerId	CustomerCode	CustomerName	CustomerPhone	Type	Unit	Value	BalancePointAfter	BalanceAmountAfter	BatchId	RefCode	Description	ExtraProperties	ConcurrencyStamp	CreationTime	CreatorId	LastModificationTime	LastModifierId	IsDeleted	DeleterId	DeletionTime
00975C6E-352E-D4B6-C26A-3A23A2154D4D	27E348A9-036C-BEF8-FA2B-3A22FC202C26	214C5A2A-3C28-849D-F4D4-3A23A2143BC7	C79N1004643	2. Nhà Thuốc Thiên Thảo	0933856863	1	2	300000.00	0.00	300000.00	1A0DC7D9-C04C-B859-EB83-3A23A2154D48	PB2609110016	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	50cf5ea757f0401680f25fffbaa9bf76	2026-09-11 16:59:31.4053280	NULL	NULL	NULL	0	NULL	NULL
FF8A3038-5CF5-13FF-4B81-3A23F948C4E3	27E348A9-036C-BEF8-FA2B-3A22FC202C26	77A32579-D1E4-F075-FDCA-3A23F948793E	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	1	2	2200000.00	0.00	2200000.00	081A723D-42A2-6E2F-75C0-3A23F948C4DD	PB2609280014	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	b12343f2de534b7080e628f194be74e6	2026-09-28 15:22:42.1487169	NULL	NULL	NULL	0	NULL	NULL
66437AF5-ABF5-5D94-6CD7-3A23FD6D17A0	27E348A9-036C-BEF8-FA2B-3A22FC202C26	1B39E13C-969F-A26F-CF42-3A23FD6C47DF	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	1	2	300000.00	0.00	300000.00	D3B5E555-B291-F5C5-22FD-3A23FD6D179B	PB2609290018	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	55e7caca98b54f68aa649e0525584eb4	2026-09-29 10:40:51.4885668	NULL	NULL	NULL	0	NULL	NULL
0170D089-5EE8-3E78-1DDF-3A23FF051CF7	27E348A9-036C-BEF8-FA2B-3A22FC202C26	19DBABB6-F07C-D314-4682-3A235369D707	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	1	2	500000.00	0.00	500000.00	6F515CA7-89C5-BDA9-2DFF-3A23FF051C67	PB2609290037	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	6f970ce03bf642d98ae6e501a14b6e05	2026-09-29 18:06:31.5961825	NULL	NULL	NULL	0	NULL	NULL
842C1FD4-DF1B-1938-53EB-3A24035AECA1	27E348A9-036C-BEF8-FA2B-3A22FC202C26	4F6F5361-D5B1-A2A6-052B-3A235396D8C7	C94N1000225	2. Nhà Thuốc Thanh Liêm (Nguyễn Thanh Hào)	0838172900	1	2	200000.00	0.00	200000.00	A5BEE32D-DC23-7AAE-76D3-3A24035AEC9C	PB2609300015	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	5559c0c8b35b492b84d6cfc1dd4f1de7	2026-09-30 14:18:44.1298769	NULL	NULL	NULL	0	NULL	NULL
DFDEA070-0D83-B594-D387-3A24035CB7B7	27E348A9-036C-BEF8-FA2B-3A22FC202C26	4F6F5361-D5B1-A2A6-052B-3A235396D8C7	C94N1000286	2. NHÀ THUỐC THANH LIÊM 2 ( Nguyễn Thanh Hào)	0838172900	1	2	500000.00	0.00	700000.00	ABCF4D8A-819B-00C5-48C5-3A24035CB7B2	PB2609300017	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	b7e39f7d69254a188b93c3225d03a913	2026-09-30 14:20:41.6554978	NULL	NULL	NULL	0	NULL	NULL
DB75D8F7-03D2-D2F2-7958-3A2403B9FE64	27E348A9-036C-BEF8-FA2B-3A22FC202C26	6671068D-1F3B-E684-0220-3A23BAB61650	C75N1000655	2. Nhà thuốc Ngọc ân (Lê Thị Ngọc Ân)	0937782277	1	2	200000.00	0.00	200000.00	E0197B39-687C-ABA4-B1E8-3A2403B9FE5F	PB2609300024	Đổi từ chiến dịch Thăng hạng tri ân 2026 - đợt 1 (THTA202601)	{}	5924bcadaa154202ba276c9784d8a9df	2026-09-30 16:02:34.5968639	NULL	NULL	NULL	0	NULL	NULL
Nhưng giao dịch đổi quà lại vượt quá hạn mức số tiền thưởng đã đổi:
Id	TenantId	ExchangeCode	CustomerCode	CustomerName	CustomerPhone	GiftName	GiftCode	GiftImageUrl	PointsRequired	Quantity	TotalPointsUsed	Status	Note	InternalNote	ApprovedBy	ApprovedAt	UrBoxVoucherCode	UrBoxResponse	DeliveryAddress	ExtraProperties	ConcurrencyStamp	CreationTime	CreatorId	LastModificationTime	LastModifierId	IsDeleted	DeleterId	DeletionTime
C8FCA4EE-F6DD-40FB-BD68-14D1D851252A	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930541A	C75N1000655	2. Nhà thuốc Ngọc ân (Lê Thị Ngọc Ân)	0937782277	[UrBox Voucher] Thẻ Nạp Mobifone 200.000 đ	3727	https://images.urbox.vn/_img_server/2021/05/07/640/1620369912_6094e1f83ceb1.png	200000	1	200000	1	NULL	UrBox transaction_id=7d0233470c1a42679912c31a3bf4abb7	NULL	2026-09-30 17:17:21.8823582	181709178561	{"done":1,"msg":"success","microtime":"0.66951s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"7d0233470c1a42679912c31a3bf4abb7","cart_created":"1790763441","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/7c28bf0bac59a2826ea2b8e27caf8490.html?cart_no=122582989","linkCombo":"","linkShippingInfo":"","cart":{"id":"122582989","cartNo":"122582989","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/616cfe49856535ec5619eac21bc2d608.html"],"code_link_gift":[{"cart_detail_id":"165561434","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/616cfe49856535ec5619eac21bc2d608.html","code":"181709178561","card_id":0,"pin":"","serial":"103337000002261","priceId":"3727","gift_id":"1553","brand_id":397,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Mobifone","token":"616cfe49856535ec5619eac21bc2d608","expired":"30\/09\/2028","expired_time":1853945999,"valid_time":1790763441,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0937782277","receive_code":"616cfe49856535ec5619eac21bc2d608","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":200000}]}}}	NULL	{}	76f9af57f3cb494bbe26f05ae92204fc	2026-09-30 17:17:21.8843525	NULL	NULL	NULL	0	NULL	NULL
FEC0E766-D324-4275-868D-2A79988EB203	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609290122	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	[UrBox Voucher] Thẻ Nạp Viettel 500.000 đ	3711	https://images.urbox.vn/_img_server/2021/05/07/640/1620371263_6094e73f8f5b3.png	500000	1	500000	1	NULL	UrBox transaction_id=ab3fa7c9facd485cb9f5d13333118d8e	NULL	2026-09-29 18:18:51.8167453	111171111113605	{"done":1,"msg":"success","microtime":"0.64815s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"ab3fa7c9facd485cb9f5d13333118d8e","cart_created":"1790680731","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/1aca8df0fe738ae741b0f84e47035d4d.html?cart_no=122550847","linkCombo":"","linkShippingInfo":"","cart":{"id":"122550847","cartNo":"122550847","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/1719a53b2efc1b25977d9f5280c0ba0d.html"],"code_link_gift":[{"cart_detail_id":"165518597","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/1719a53b2efc1b25977d9f5280c0ba0d.html","code":"111171111113605","card_id":0,"pin":"","serial":"10011621122936","priceId":"3711","gift_id":"1551","brand_id":395,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Viettel","token":"1719a53b2efc1b25977d9f5280c0ba0d","expired":"30\/03\/2028","expired_time":1838048399,"valid_time":1790680731,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0987380790","receive_code":"1719a53b2efc1b25977d9f5280c0ba0d","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":500000}]}}}	NULL	{}	daaf761abe544adda7d4b3322a22b232	2026-09-29 18:18:51.8189375	NULL	NULL	NULL	1	NULL	NULL
7773C077-43BA-465F-83B8-2E4DF38C5B2E	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930D3AE	C94N1000225	2. Nhà Thuốc Thanh Liêm (Nguyễn Thanh Hào)	0838172900	[UrBox Voucher] MUJI 200.000 đ	19170	https://images.urbox.vn/_img_server/2025/08/14/640/1755158145_689d968224daf.png	200000	1	200000	1	NULL	UrBox transaction_id=a4838594c57c45e18742287636a536c7	NULL	2026-09-30 14:19:23.0606467	UB741776365	{"done":1,"msg":"success","microtime":"0.62856s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"a4838594c57c45e18742287636a536c7","cart_created":"1790752762","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/ac40fc9d5d141539724adeee153e4de3.html?cart_no=122576228","linkCombo":"","linkShippingInfo":"","cart":{"id":"122576228","cartNo":"122576228","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/2e0c84a996975af433263771084e3618.html"],"code_link_gift":[{"cart_detail_id":"165553992","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/2e0c84a996975af433263771084e3618.html","code":"UB741776365","card_id":0,"pin":"","serial":"","priceId":"19170","gift_id":"14091","brand_id":2008,"supplier_id":1137,"gift_title":"[UrBox Voucher] MUJI","token":"2e0c84a996975af433263771084e3618","expired":"29\/12\/2026","expired_time":1798563599,"valid_time":1790752762,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB741776365","estimateDelivery":"","ttemail":"","ttphone":"0838172900","receive_code":"2e0c84a996975af433263771084e3618","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	c74a8d38376c427fa39cf8573e10bdc4	2026-09-30 14:19:23.0624675	NULL	NULL	NULL	0	NULL	NULL
D93C6E11-5008-4366-BEE2-3F6B4182ACD5	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-26092902B4	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	[UrBox Voucher] Thẻ Nạp Viettel 500.000 đ	3711	https://images.urbox.vn/_img_server/2021/05/07/640/1620371263_6094e73f8f5b3.png	500000	1	500000	1	NULL	UrBox transaction_id=b8dba5cdd7d044b7bcdae9fe17771f53	NULL	2026-09-29 18:21:08.0146772	810219347785332	{"done":1,"msg":"success","microtime":"0.66545s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"b8dba5cdd7d044b7bcdae9fe17771f53","cart_created":"1790680867","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/379dd92e27af3df618414ad0ad309d41.html?cart_no=122550927","linkCombo":"","linkShippingInfo":"","cart":{"id":"122550927","cartNo":"122550927","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/297b8e9c7bffa0c8c35c581b8a900938.html"],"code_link_gift":[{"cart_detail_id":"165518684","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/297b8e9c7bffa0c8c35c581b8a900938.html","code":"810219347785332","card_id":0,"pin":"","serial":"10011621122933","priceId":"3711","gift_id":"1551","brand_id":395,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Viettel","token":"297b8e9c7bffa0c8c35c581b8a900938","expired":"30\/03\/2028","expired_time":1838048399,"valid_time":1790680867,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0987380790","receive_code":"297b8e9c7bffa0c8c35c581b8a900938","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":500000}]}}}	NULL	{}	b5dc1bca54974a34b0e01da00661fa5b	2026-09-29 18:21:08.0166464	NULL	NULL	NULL	0	NULL	NULL
6C8D2935-56B7-49DC-A0F1-42F1B2E8DA65	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-26092829C5	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	[UrBox Voucher] WinMart/ WinMart+/WiN 200.000 đ	13342	https://images.urbox.vn/_img_server/2026/05/18/640/1779116803_6a0b2b03f1cbb.png	200000	1	200000	1	NULL	UrBox transaction_id=19abfdbe47064ef8b566512856cca791	NULL	2026-09-28 15:29:19.9178493	UB369076849	{"done":1,"msg":"success","microtime":"0.61143s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"19abfdbe47064ef8b566512856cca791","cart_created":"1790584159","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/3973b15e2cb49b526192ae9796916c08.html?cart_no=122440704","linkCombo":"","linkShippingInfo":"","cart":{"id":"122440704","cartNo":"122440704","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/7296e71efc905913c63e7a01820da317.html"],"code_link_gift":[{"cart_detail_id":"165400215","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/7296e71efc905913c63e7a01820da317.html","code":"UB369076849","card_id":0,"pin":"","serial":"","priceId":"13342","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"7296e71efc905913c63e7a01820da317","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790584159,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB369076849","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"7296e71efc905913c63e7a01820da317","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	aa970116785f4344851bf54a537e131b	2026-09-28 15:29:19.9197514	NULL	NULL	NULL	0	NULL	NULL
9FD6DB8B-30C8-4E86-AD12-45F831F0B530	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930B746	C94N1000225	2. Nhà Thuốc Thanh Liêm (Nguyễn Thanh Hào)	0838172900	[UrBox Voucher] MUJI 200.000 đ	19170	https://images.urbox.vn/_img_server/2025/08/14/640/1755158145_689d968224daf.png	200000	1	200000	1	NULL	UrBox transaction_id=be702d8f0d634206b2cb362b37358b91	NULL	2026-09-30 14:20:06.2910766	UB305816641	{"done":1,"msg":"success","microtime":"0.72933s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"be702d8f0d634206b2cb362b37358b91","cart_created":"1790752805","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/c82323adfc97dd2a4f6accf5d7f4ea8b.html?cart_no=122576252","linkCombo":"","linkShippingInfo":"","cart":{"id":"122576252","cartNo":"122576252","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/33659b1d71d11d3cb6c9b1c5b4493d57.html"],"code_link_gift":[{"cart_detail_id":"165554019","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/33659b1d71d11d3cb6c9b1c5b4493d57.html","code":"UB305816641","card_id":0,"pin":"","serial":"","priceId":"19170","gift_id":"14091","brand_id":2008,"supplier_id":1137,"gift_title":"[UrBox Voucher] MUJI","token":"33659b1d71d11d3cb6c9b1c5b4493d57","expired":"29\/12\/2026","expired_time":1798563599,"valid_time":1790752805,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB305816641","estimateDelivery":"","ttemail":"","ttphone":"0838172900","receive_code":"33659b1d71d11d3cb6c9b1c5b4493d57","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	82c3280ca53f48f68bbe3e5ca5aae0cf	2026-09-30 14:20:06.2936561	NULL	NULL	NULL	1	NULL	NULL
97613361-14E4-483E-AE9D-475EF9906132	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929C487	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 200.000 đ	13059	https://images.urbox.vn/_img_server/2026/05/19/640/1779162948_6a0bdf445404c.png	200000	1	200000	1	NULL	UrBox transaction_id=afd3ad03f13c4dd2880bd08554fdca3c	NULL	2026-09-29 13:30:02.7905889	UB846749912	{"done":1,"msg":"success","microtime":"0.88282s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"afd3ad03f13c4dd2880bd08554fdca3c","cart_created":"1790663402","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/cd8525e6d2cdbda06a46c3569d9a68c2.html?cart_no=122538691","linkCombo":"","linkShippingInfo":"","cart":{"id":"122538691","cartNo":"122538691","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/b6abcf4d1d4419494b1831ce4175cc0f.html"],"code_link_gift":[{"cart_detail_id":"165505377","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/b6abcf4d1d4419494b1831ce4175cc0f.html","code":"UB846749912","card_id":0,"pin":"","serial":"","priceId":"13059","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"b6abcf4d1d4419494b1831ce4175cc0f","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790663402,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB846749912","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"b6abcf4d1d4419494b1831ce4175cc0f","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	db6a81deac9c4114ae8260dc1209af18	2026-09-29 13:30:02.7930307	NULL	NULL	NULL	1	NULL	NULL
07B374D0-A945-4C52-870B-4D293487526A	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929612F	C79N1004643	2. Nhà Thuốc Thiên Thảo	0933856863	[UrBox Voucher] Co.opmart 100.000 đ	10595	https://images.urbox.vn/_img_server/2026/05/18/640/1779096378_6a0adb3a9d20f.png	100000	1	100000	1	NULL	UrBox transaction_id=eb9328153f01414db1a1988da78221c6	NULL	2026-09-29 08:11:04.3269962	UB127929736	{"done":1,"msg":"success","microtime":"0.64497s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"eb9328153f01414db1a1988da78221c6","cart_created":"1790644263","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/59a965c39a0a9148e679a21f9790a30e.html?cart_no=122523089","linkCombo":"","linkShippingInfo":"","cart":{"id":"122523089","cartNo":"122523089","money_total":"100000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/076d16bda37e22f9454ff69a06606c92.html"],"code_link_gift":[{"cart_detail_id":"165487268","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/076d16bda37e22f9454ff69a06606c92.html","code":"UB127929736","card_id":0,"pin":"","serial":"","priceId":"10595","gift_id":"7207","brand_id":1170,"supplier_id":641,"gift_title":"[UrBox Voucher] Co.opmart","token":"076d16bda37e22f9454ff69a06606c92","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790644264,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB127929736","estimateDelivery":"","ttemail":"","ttphone":"0933856863","receive_code":"076d16bda37e22f9454ff69a06606c92","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":100000}]}}}	NULL	{}	bea9f781c70140a5ba30725a5df9ae82	2026-09-29 08:11:04.3288966	NULL	NULL	NULL	0	NULL	NULL
3DDC120F-46B5-497C-80F8-5884CEB66AD7	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-26092922CD	C79N1004643	2. Nhà Thuốc Thiên Thảo	0933856863	[UrBox Voucher] GO! & Mini GO! 200.000 đ	8692	https://images.urbox.vn/_img_server/2026/03/17/640/1773718908_69b8cd7c544e9.png	200000	1	200000	1	NULL	UrBox transaction_id=931ae9f226254012b17de290bb1f0f33	NULL	2026-09-29 08:13:59.6674472	UB271850384	{"done":1,"msg":"success","microtime":"0.63199s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"931ae9f226254012b17de290bb1f0f33","cart_created":"1790644439","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/960988297340b467f7563818e7909197.html?cart_no=122523251","linkCombo":"","linkShippingInfo":"","cart":{"id":"122523251","cartNo":"122523251","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/12eab4f7278f5b39fd03a3a9fbe510a5.html"],"code_link_gift":[{"cart_detail_id":"165487909","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/12eab4f7278f5b39fd03a3a9fbe510a5.html","code":"UB271850384","card_id":0,"pin":"","serial":"","priceId":"8692","gift_id":"5725","brand_id":910,"supplier_id":517,"gift_title":"[UrBox Voucher] GO&#33; & Mini GO&#33;","token":"12eab4f7278f5b39fd03a3a9fbe510a5","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790644439,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB271850384","estimateDelivery":"","ttemail":"","ttphone":"0933856863","receive_code":"12eab4f7278f5b39fd03a3a9fbe510a5","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	7dabe6c073bb4e2b9e64830b857af74a	2026-09-29 08:13:59.6693957	NULL	NULL	NULL	1	NULL	NULL
32B3A8A8-6206-47F6-BF4F-62AB56D4D0CB	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609281EFA	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	[UrBox Voucher] WinMart/ WinMart+/WiN 1.000.000 đ	14628	https://images.urbox.vn/_img_server/2026/05/18/640/1779117048_6a0b2bf84ad23.png	1000000	1	1000000	1	NULL	UrBox transaction_id=3734587d919f451f906b50717b3bb0ad	NULL	2026-09-28 15:25:22.7657786	UB928734684	{"done":1,"msg":"success","microtime":"0.72741s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"3734587d919f451f906b50717b3bb0ad","cart_created":"1790583922","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/c861fe78d0931c102da6a54e3ef1c2f6.html?cart_no=122438857","linkCombo":"","linkShippingInfo":"","cart":{"id":"122438857","cartNo":"122438857","money_total":"1000000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/5d03ba1442696bb7eebef2bc17e64bed.html"],"code_link_gift":[{"cart_detail_id":"165398300","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/5d03ba1442696bb7eebef2bc17e64bed.html","code":"UB928734684","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"5d03ba1442696bb7eebef2bc17e64bed","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790583922,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB928734684","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"5d03ba1442696bb7eebef2bc17e64bed","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000}]}}}	NULL	{}	cf8b6bdd7756488ba4dcf1ccb0875448	2026-09-28 15:25:22.7677189	NULL	NULL	NULL	1	NULL	NULL
6F901004-4ECB-451C-BD29-6445ED423BFA	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929AF15	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 200.000 đ	13059	https://images.urbox.vn/_img_server/2026/05/19/640/1779162948_6a0bdf445404c.png	200000	1	200000	1	NULL	UrBox transaction_id=62aa800b30384a08a4ae750c405c3f90	NULL	2026-09-29 11:49:04.0804586	UB233984456	{"done":1,"msg":"success","microtime":"0.80228s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"62aa800b30384a08a4ae750c405c3f90","cart_created":"1790657343","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/eee0b6a874fae82418aca24252e118e8.html?cart_no=122535455","linkCombo":"","linkShippingInfo":"","cart":{"id":"122535455","cartNo":"122535455","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/34474fab79328e5926a921f0e9786412.html"],"code_link_gift":[{"cart_detail_id":"165501841","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/34474fab79328e5926a921f0e9786412.html","code":"UB233984456","card_id":0,"pin":"","serial":"","priceId":"13059","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"34474fab79328e5926a921f0e9786412","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790657343,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB233984456","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"34474fab79328e5926a921f0e9786412","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	74cce6cce11d49b18b462050b49a8b3f	2026-09-29 11:49:04.0823018	NULL	NULL	NULL	1	NULL	NULL
AEDE4194-CD04-4955-A05F-652E69C86CE3	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260928B5EF	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	[UrBox Voucher] WinMart/ WinMart+/WiN 1.000.000 đ	14628	https://images.urbox.vn/_img_server/2026/05/18/640/1779117048_6a0b2bf84ad23.png	2000000	2	2000000	1	NULL	UrBox transaction_id=1fcd6c958ef240a9be17649f3be9c67b	NULL	2026-09-28 15:27:34.4022505	UB357904099	{"done":1,"msg":"success","microtime":"1.01844s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"1fcd6c958ef240a9be17649f3be9c67b","cart_created":"1790584053","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/52ca94ee6cb1af0d59349afd7cd65108.html?cart_no=122439918","linkCombo":"","linkShippingInfo":"","cart":{"id":"122439918","cartNo":"122439918","money_total":"2000000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/289840b6bde9c03c27af1907afbc9ab2.html","https:\/\/urbox.vn\/nhan-qua\/67e4f384382288575220b5175cd1a714.html"],"code_link_gift":[{"cart_detail_id":"165399422","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/289840b6bde9c03c27af1907afbc9ab2.html","code":"UB357904099","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"289840b6bde9c03c27af1907afbc9ab2","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790584053,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB357904099","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"289840b6bde9c03c27af1907afbc9ab2","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000},{"cart_detail_id":"165399423","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/67e4f384382288575220b5175cd1a714.html","code":"UB559439712","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"67e4f384382288575220b5175cd1a714","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790584053,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB559439712","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"67e4f384382288575220b5175cd1a714","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000}]}}}	NULL	{}	2ec03b3477504b85bdbfde4e11f0cf59	2026-09-28 15:27:34.4042894	NULL	NULL	NULL	1	NULL	NULL
8A7AEB67-24CD-4612-8784-7675F4B32FBD	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609294560	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 100.000 đ	13058	https://images.urbox.vn/_img_server/2026/05/19/640/1779162895_6a0bdf0fc6848.png	100000	1	100000	1	NULL	UrBox transaction_id=29df90e90aa640229c8ea355a59e589d	NULL	2026-09-29 11:38:53.9774651	UB652699488	{"done":1,"msg":"success","microtime":"0.66046s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"29df90e90aa640229c8ea355a59e589d","cart_created":"1790656733","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/a78a19f997932f19514d3b8be0fe089b.html?cart_no=122535081","linkCombo":"","linkShippingInfo":"","cart":{"id":"122535081","cartNo":"122535081","money_total":"100000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/6fc36647681fd2b7ccfd8851da0476d7.html"],"code_link_gift":[{"cart_detail_id":"165501439","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/6fc36647681fd2b7ccfd8851da0476d7.html","code":"UB652699488","card_id":0,"pin":"","serial":"","priceId":"13058","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"6fc36647681fd2b7ccfd8851da0476d7","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790656733,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB652699488","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"6fc36647681fd2b7ccfd8851da0476d7","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":100000}]}}}	NULL	{}	ea21a67b67a34927bfe4e3bf3c9b6ee4	2026-09-29 11:38:53.9793613	NULL	NULL	NULL	1	NULL	NULL
40D202DF-29EC-4172-A81A-82C3A87F0724	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609283A29	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	[UrBox Voucher] WinMart/ WinMart+/WiN 1.000.000 đ	14628	https://images.urbox.vn/_img_server/2026/05/18/640/1779117048_6a0b2bf84ad23.png	2000000	2	2000000	1	NULL	UrBox transaction_id=40394a0fd0c743b2bf5932932884c168	NULL	2026-09-28 15:28:31.9720138	UB278790732	{"done":1,"msg":"success","microtime":"0.97251s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"40394a0fd0c743b2bf5932932884c168","cart_created":"1790584111","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/bb4888b33c1d3e390c7dd24585b72c5f.html?cart_no=122440366","linkCombo":"","linkShippingInfo":"","cart":{"id":"122440366","cartNo":"122440366","money_total":"2000000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/f747f2970f2501a851b30bab04de8bd9.html","https:\/\/urbox.vn\/nhan-qua\/e390407c82a7335cb9fde51e94e42a79.html"],"code_link_gift":[{"cart_detail_id":"165399875","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/f747f2970f2501a851b30bab04de8bd9.html","code":"UB278790732","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"f747f2970f2501a851b30bab04de8bd9","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790584111,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB278790732","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"f747f2970f2501a851b30bab04de8bd9","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000},{"cart_detail_id":"165399876","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/e390407c82a7335cb9fde51e94e42a79.html","code":"UB280633537","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"e390407c82a7335cb9fde51e94e42a79","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790584111,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB280633537","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"e390407c82a7335cb9fde51e94e42a79","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000}]}}}	NULL	{}	61fef28455954b288e71c8c7d470dff2	2026-09-28 15:28:31.9738994	NULL	NULL	NULL	0	NULL	NULL
446F0E36-39F8-4175-91A7-86FE56D97F65	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929D8D8	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	[UrBox Voucher] Thẻ Nạp Viettel 500.000 đ	3711	https://images.urbox.vn/_img_server/2021/05/07/640/1620371263_6094e73f8f5b3.png	500000	1	500000	1	NULL	UrBox transaction_id=89676cf23af141219077d816d46e0b85	NULL	2026-09-29 18:20:13.2270468	418465357143392	{"done":1,"msg":"success","microtime":"0.58365s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"89676cf23af141219077d816d46e0b85","cart_created":"1790680812","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/dd8a6a759b077991282addfaaf36c2b9.html?cart_no=122550894","linkCombo":"","linkShippingInfo":"","cart":{"id":"122550894","cartNo":"122550894","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/1176402002c7b434c9281324c5bc4da0.html"],"code_link_gift":[{"cart_detail_id":"165518647","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/1176402002c7b434c9281324c5bc4da0.html","code":"418465357143392","card_id":0,"pin":"","serial":"10011621122935","priceId":"3711","gift_id":"1551","brand_id":395,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Viettel","token":"1176402002c7b434c9281324c5bc4da0","expired":"30\/03\/2028","expired_time":1838048399,"valid_time":1790680812,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0987380790","receive_code":"1176402002c7b434c9281324c5bc4da0","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":500000}]}}}	NULL	{}	7c648ee3cfa6429298c796f82f26c173	2026-09-29 18:20:13.2291357	NULL	NULL	NULL	1	NULL	NULL
EF5BE0AC-B671-40F8-AB36-9A22C6692477	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930EA24	C75N1000655	2. Nhà thuốc Ngọc ân (Lê Thị Ngọc Ân)	0937782277	[UrBox Voucher] Thẻ Nạp Mobifone 200.000 đ	3727	https://images.urbox.vn/_img_server/2021/05/07/640/1620369912_6094e1f83ceb1.png	200000	1	200000	1	NULL	UrBox transaction_id=d16339ab54ea477e934c5b14a661f666	NULL	2026-09-30 17:24:29.2130812	652915843141	{"done":1,"msg":"success","microtime":"0.76103s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"d16339ab54ea477e934c5b14a661f666","cart_created":"1790763868","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/a55f4fe8bba355fd41c556dd77ff3228.html?cart_no=122583287","linkCombo":"","linkShippingInfo":"","cart":{"id":"122583287","cartNo":"122583287","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/8e8b90fb4e18e59cd46962685d9f0593.html"],"code_link_gift":[{"cart_detail_id":"165561744","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/8e8b90fb4e18e59cd46962685d9f0593.html","code":"652915843141","card_id":0,"pin":"","serial":"103337000002041","priceId":"3727","gift_id":"1553","brand_id":397,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Mobifone","token":"8e8b90fb4e18e59cd46962685d9f0593","expired":"30\/09\/2028","expired_time":1853945999,"valid_time":1790763868,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0937782277","receive_code":"8e8b90fb4e18e59cd46962685d9f0593","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":200000}]}}}	NULL	{}	e0e2bf4b09b44fe799459f177efb3cd7	2026-09-30 17:24:29.2207293	NULL	NULL	NULL	1	NULL	NULL
14D8E10D-91FA-4362-8CB9-9A259B4F927E	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609301392	C94N1000225	2. Nhà Thuốc Thanh Liêm (Nguyễn Thanh Hào)	0838172900	[UrBox Voucher] MUJI 500.000 đ	19171	https://images.urbox.vn/_img_server/2025/08/14/640/1755158175_689d96a011748.png	500000	1	500000	1	NULL	UrBox transaction_id=ef3c6191ee9b48559274694f96e3f22b	NULL	2026-09-30 14:30:10.7527021	UB905875649	{"done":1,"msg":"success","microtime":"0.77553s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"ef3c6191ee9b48559274694f96e3f22b","cart_created":"1790753410","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/42603583c23208ccb20f004e70bd7ce0.html?cart_no=122576518","linkCombo":"","linkShippingInfo":"","cart":{"id":"122576518","cartNo":"122576518","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/9961e8fcd60abc700d528b8ad5d7cfdf.html"],"code_link_gift":[{"cart_detail_id":"165554356","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/9961e8fcd60abc700d528b8ad5d7cfdf.html","code":"UB905875649","card_id":0,"pin":"","serial":"","priceId":"19171","gift_id":"14091","brand_id":2008,"supplier_id":1137,"gift_title":"[UrBox Voucher] MUJI","token":"9961e8fcd60abc700d528b8ad5d7cfdf","expired":"29\/12\/2026","expired_time":1798563599,"valid_time":1790753410,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB905875649","estimateDelivery":"","ttemail":"","ttphone":"0838172900","receive_code":"9961e8fcd60abc700d528b8ad5d7cfdf","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":500000}]}}}	NULL	{}	c6665084086f4590bce8eed3664440e6	2026-09-30 14:30:10.7549495	NULL	NULL	NULL	1	NULL	NULL
E12A6F30-6402-4B87-A885-ADAE470A8E74	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260928BBE0	C48N1001594	1. Công ty TNHH Hoàn Thành Vũ	0905556117	[UrBox Voucher] WinMart/ WinMart+/WiN 1.000.000 đ	14628	https://images.urbox.vn/_img_server/2026/05/18/640/1779117048_6a0b2bf84ad23.png	2000000	2	2000000	1	NULL	UrBox transaction_id=ff6ef498156a48debac779e466795e4c	NULL	2026-09-28 15:26:11.1364067	UB447112260	{"done":1,"msg":"success","microtime":"1.46676s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"ff6ef498156a48debac779e466795e4c","cart_created":"1790583970","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/2934e1046324e11338a18cdb56eb2887.html?cart_no=122439251","linkCombo":"","linkShippingInfo":"","cart":{"id":"122439251","cartNo":"122439251","money_total":"2000000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/075ee370f84d21dcbf476d6e8d962d74.html","https:\/\/urbox.vn\/nhan-qua\/625948e9d8a4a98010a50e53a81bce70.html"],"code_link_gift":[{"cart_detail_id":"165398724","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/075ee370f84d21dcbf476d6e8d962d74.html","code":"UB447112260","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"075ee370f84d21dcbf476d6e8d962d74","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790583970,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB447112260","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"075ee370f84d21dcbf476d6e8d962d74","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000},{"cart_detail_id":"165398725","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/625948e9d8a4a98010a50e53a81bce70.html","code":"UB754508244","card_id":0,"pin":"","serial":"","priceId":"14628","gift_id":"9282","brand_id":1511,"supplier_id":845,"gift_title":"[UrBox Voucher] WinMart\/ WinMart+\/WiN","token":"625948e9d8a4a98010a50e53a81bce70","expired":"27\/12\/2026","expired_time":1798390799,"valid_time":1790583970,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB754508244","estimateDelivery":"","ttemail":"","ttphone":"0905556117","receive_code":"625948e9d8a4a98010a50e53a81bce70","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":1000000}]}}}	NULL	{}	b9c31a78328844ac92ac732e845dfab3	2026-09-28 15:26:11.1384599	NULL	NULL	NULL	1	NULL	NULL
E63F7C21-EC93-428B-9DE5-BA55A65A7178	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609292850	C79N1004643	2. Nhà Thuốc Thiên Thảo	0933856863	[UrBox Voucher] Co.opmart 200.000 đ	10596	https://images.urbox.vn/_img_server/2026/05/18/640/1779096412_6a0adb5cb924a.png	200000	1	200000	1	NULL	UrBox transaction_id=6aa1a782ef1045c9b499a3a6af2d222c	NULL	2026-09-29 08:10:27.2216675	UB208156392	{"done":1,"msg":"success","microtime":"0.63596s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"6aa1a782ef1045c9b499a3a6af2d222c","cart_created":"1790644226","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/51f8af9ac55c4e2db5d15403ff14b64d.html?cart_no=122523050","linkCombo":"","linkShippingInfo":"","cart":{"id":"122523050","cartNo":"122523050","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/24795e9da468aef5603fd68bc61f913f.html"],"code_link_gift":[{"cart_detail_id":"165487125","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/24795e9da468aef5603fd68bc61f913f.html","code":"UB208156392","card_id":0,"pin":"","serial":"","priceId":"10596","gift_id":"7207","brand_id":1170,"supplier_id":641,"gift_title":"[UrBox Voucher] Co.opmart","token":"24795e9da468aef5603fd68bc61f913f","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790644226,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB208156392","estimateDelivery":"","ttemail":"","ttphone":"0933856863","receive_code":"24795e9da468aef5603fd68bc61f913f","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	7c125a57878643d8b18cb9306f53f5fa	2026-09-29 08:10:27.2235501	NULL	NULL	NULL	0	NULL	NULL
CCCEE0A6-064A-4371-9D2A-C5A3B4F9405E	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930AE8C	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 100.000 đ	13058	https://images.urbox.vn/_img_server/2026/05/19/640/1779162895_6a0bdf0fc6848.png	100000	1	100000	1	NULL	UrBox transaction_id=e283be5b77e64fbaa5cc430bfa59e0d0	NULL	2026-09-30 13:42:06.9973758	UB977149037	{"done":1,"msg":"success","microtime":"0.60533s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"e283be5b77e64fbaa5cc430bfa59e0d0","cart_created":"1790750526","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/cff7647c7947788f70cf4f7b0bb22bb1.html?cart_no=122575097","linkCombo":"","linkShippingInfo":"","cart":{"id":"122575097","cartNo":"122575097","money_total":"100000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/b8a0d06ab22a89c5772095a1dba48f87.html"],"code_link_gift":[{"cart_detail_id":"165552679","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/b8a0d06ab22a89c5772095a1dba48f87.html","code":"UB977149037","card_id":0,"pin":"","serial":"","priceId":"13058","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"b8a0d06ab22a89c5772095a1dba48f87","expired":"29\/12\/2026","expired_time":1798563599,"valid_time":1790750526,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB977149037","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"b8a0d06ab22a89c5772095a1dba48f87","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":100000}]}}}	NULL	{}	09059b882a214433a248264bb32b96b6	2026-09-30 13:42:06.9999161	NULL	NULL	NULL	0	NULL	NULL
06ACEC36-DE3B-4B93-A2B4-CD13E4BFB2D1	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929B917	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	[UrBox Voucher] Thẻ Nạp Viettel 500.000 đ	3711	https://images.urbox.vn/_img_server/2021/05/07/640/1620371263_6094e73f8f5b3.png	500000	1	500000	1	NULL	UrBox transaction_id=c9b59c9f16f54698b2181f3d2ce15309	NULL	2026-09-29 18:07:10.3778011	519859151205752	{"done":1,"msg":"success","microtime":"0.64643s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"c9b59c9f16f54698b2181f3d2ce15309","cart_created":"1790680029","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/d356981d91cccd3011aaf4ea1e109ba8.html?cart_no=122550394","linkCombo":"","linkShippingInfo":"","cart":{"id":"122550394","cartNo":"122550394","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/5cb2bab6f0ff7451f24e6aa98c82fdbe.html"],"code_link_gift":[{"cart_detail_id":"165518095","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/5cb2bab6f0ff7451f24e6aa98c82fdbe.html","code":"519859151205752","card_id":0,"pin":"","serial":"10011621122938","priceId":"3711","gift_id":"1551","brand_id":395,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Viettel","token":"5cb2bab6f0ff7451f24e6aa98c82fdbe","expired":"30\/03\/2028","expired_time":1838048399,"valid_time":1790680030,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0987380790","receive_code":"5cb2bab6f0ff7451f24e6aa98c82fdbe","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":500000}]}}}	NULL	{}	2ced1a1365aa4734a7d8297976833e33	2026-09-29 18:07:10.4432220	NULL	NULL	NULL	1	NULL	NULL
DF23D2B3-6818-4896-BDD4-DEC783003B71	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-2609292B39	C77N1000400	1. Công ty cổ phần Dược - Mỹ Phẩm & Thương Mại Bà Rịa - Vũng Tàu (Nguyễn Xuân Thủy)	0987380790	[UrBox Voucher] Thẻ Nạp Viettel 500.000 đ	3711	https://images.urbox.vn/_img_server/2021/05/07/640/1620371263_6094e73f8f5b3.png	500000	1	500000	1	NULL	UrBox transaction_id=7117cc80b63b45889cc03b7aac863335	NULL	2026-09-29 18:20:46.2336253	614643818094667	{"done":1,"msg":"success","microtime":"0.69419s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"7117cc80b63b45889cc03b7aac863335","cart_created":"1790680845","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/33a9c9dc975265a224532ac0d9985969.html?cart_no=122550916","linkCombo":"","linkShippingInfo":"","cart":{"id":"122550916","cartNo":"122550916","money_total":"500000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/ab6d97dad85e8a5a0a5d0b0f2adec9a4.html"],"code_link_gift":[{"cart_detail_id":"165518670","code_display":"Text","code_display_type":4,"link":"https:\/\/urbox.vn\/nhan-qua\/ab6d97dad85e8a5a0a5d0b0f2adec9a4.html","code":"614643818094667","card_id":0,"pin":"","serial":"10011621122934","priceId":"3711","gift_id":"1551","brand_id":395,"supplier_id":112,"gift_title":"[UrBox Voucher] Th\u1ebb N\u1ea1p Viettel","token":"ab6d97dad85e8a5a0a5d0b0f2adec9a4","expired":"30\/03\/2028","expired_time":1838048399,"valid_time":1790680845,"code_image":"","estimateDelivery":"","ttemail":"","ttphone":"0987380790","receive_code":"ab6d97dad85e8a5a0a5d0b0f2adec9a4","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":5,"urcard_id":0,"price":500000}]}}}	NULL	{}	a3553ee1c3814491851b2de46ff03650	2026-09-29 18:20:46.2355172	NULL	NULL	NULL	1	NULL	NULL
0F83DE1C-9322-4E1E-A043-E110824A36CC	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929DEA1	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 100.000 đ	13058	https://images.urbox.vn/_img_server/2026/05/19/640/1779162895_6a0bdf0fc6848.png	100000	1	100000	1	NULL	UrBox transaction_id=f79a6bc1942c4fe29a65120576ca7c81	NULL	2026-09-29 13:31:21.3638876	UB418386507	{"done":1,"msg":"success","microtime":"0.7253s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"f79a6bc1942c4fe29a65120576ca7c81","cart_created":"1790663480","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/6e36fea482a2cf826426a450420a5f55.html?cart_no=122538734","linkCombo":"","linkShippingInfo":"","cart":{"id":"122538734","cartNo":"122538734","money_total":"100000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/2be261f1762e31dee2d514b8771e3046.html"],"code_link_gift":[{"cart_detail_id":"165505428","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/2be261f1762e31dee2d514b8771e3046.html","code":"UB418386507","card_id":0,"pin":"","serial":"","priceId":"13058","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"2be261f1762e31dee2d514b8771e3046","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790663481,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB418386507","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"2be261f1762e31dee2d514b8771e3046","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":100000}]}}}	NULL	{}	dbbbb5b39fec4b8c8f1aeb7c5fe862e6	2026-09-29 13:31:21.3660359	NULL	NULL	NULL	1	NULL	NULL
AF396179-9998-4EB0-BE7F-EBE56DDCEE34	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260930F27D	C75N1000114	2. Quầy thuốc Thảo An (Phạm Thị Mai Thảo)	0986172373	[UrBox Voucher] Bách Hóa Xanh 200.000 đ	13059	https://images.urbox.vn/_img_server/2026/05/19/640/1779162948_6a0bdf445404c.png	200000	1	200000	1	NULL	UrBox transaction_id=e41674e5854b43f9be0cadc0def9413e	NULL	2026-09-30 13:40:59.6806288	UB873648602	{"done":1,"msg":"success","microtime":"0.59169s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"e41674e5854b43f9be0cadc0def9413e","cart_created":"1790750459","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/ad935e6d09d4225224d429f019038fc8.html?cart_no=122575081","linkCombo":"","linkShippingInfo":"","cart":{"id":"122575081","cartNo":"122575081","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/047948cc2c68a15dc154f94680b16008.html"],"code_link_gift":[{"cart_detail_id":"165552657","code_display":"Barcode 128","code_display_type":2,"link":"https:\/\/urbox.vn\/nhan-qua\/047948cc2c68a15dc154f94680b16008.html","code":"UB873648602","card_id":0,"pin":"","serial":"","priceId":"13059","gift_id":"9153","brand_id":138,"supplier_id":128,"gift_title":"[UrBox Voucher] B\u00e1ch H\u00f3a Xanh","token":"047948cc2c68a15dc154f94680b16008","expired":"29\/12\/2026","expired_time":1798563599,"valid_time":1790750459,"code_image":"https:\/\/apib.urbox.vn\/\/v1\/api\/barcode?type=code128&code=UB873648602","estimateDelivery":"","ttemail":"","ttphone":"0986172373","receive_code":"047948cc2c68a15dc154f94680b16008","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	c427d7061ef34e3eb87eb5480e64c975	2026-09-30 13:40:59.6873989	NULL	NULL	NULL	0	NULL	NULL
94D4459B-CA3B-42D7-87AC-FD384848C573	27E348A9-036C-BEF8-FA2B-3A22FC202C26	UB-260929CB64	C79N1004643	2. Nhà Thuốc Thiên Thảo	0933856863	[UrBox Voucher] Co.opmart 200.000 đ	10596	https://images.urbox.vn/_img_server/2026/05/18/640/1779096412_6a0adb5cb924a.png	200000	1	200000	1	NULL	UrBox transaction_id=a1c140c1e60e43d1912181e4b0a7edb7	NULL	2026-09-29 08:12:03.5936536	UB681514101	{"done":1,"msg":"success","microtime":"0.61088s","status":200,"data":{"pay":2,"app_id":1908,"transaction_id":"a1c140c1e60e43d1912181e4b0a7edb7","cart_created":"1790644323","campaign_code":"UG949779","linkCart":"https:\/\/urbox.vn\/cart\/5a12ef617f0682923f6188f894ecfb98.html?cart_no=122523142","linkCombo":"","linkShippingInfo":"","cart":{"id":"122523142","cartNo":"122523142","money_total":"200000","money_ship":"0","link_gift":["https:\/\/urbox.vn\/nhan-qua\/2cd7b1b2f10c20872f4ba57270c393d0.html"],"code_link_gift":[{"cart_detail_id":"165487487","code_display":"Barcode, QRcode","code_display_type":5,"link":"https:\/\/urbox.vn\/nhan-qua\/2cd7b1b2f10c20872f4ba57270c393d0.html","code":"UB681514101","card_id":0,"pin":"","serial":"","priceId":"10596","gift_id":"7207","brand_id":1170,"supplier_id":641,"gift_title":"[UrBox Voucher] Co.opmart","token":"2cd7b1b2f10c20872f4ba57270c393d0","expired":"28\/12\/2026","expired_time":1798477199,"valid_time":1790644323,"code_image":"https:\/\/apib.urbox.vn\/v1\/api\/qrbarcode?code=UB681514101","estimateDelivery":"","ttemail":"","ttphone":"0933856863","receive_code":"2cd7b1b2f10c20872f4ba57270c393d0","city_id":0,"district_id":0,"ward_id":0,"delivery_note":"","ttaddress":"","deliveryCode":1,"type":1,"urcard_id":0,"price":200000}]}}}	NULL	{}	d16eb0bfb99a478899180e042fe885b6	2026-09-29 08:12:03.5955968	NULL	NULL	NULL	0	NULL	NULL
- Tôi đã phải chặn các giao dịch để khách hàng không thể sử dụng được quà vì gây sai lệch với nhà cung cấp quà (lệch phải đền bù tiền thiệt hại).
- Hiện tại khả năng khách đổi vượt hạn mức vẫn có thể đang tiếp diễn.

2. Các API call để lấy thông tin đổi thưởng từ hệ thống Hoa Linh DMS (lưu ý chỉ lấy bản ghi có campaign_code = "THTA202601" và campaign_name = "Thăng hạng tri ân 2026 - đợt 1") và tôi chỉ gửi API lấy ra những khách hàng có tiền thưởng để đổi (1 số điện thoại có 1 hoặc nhiều mã khách hàng và một trong những khách hàng đủ điều kiện sẽ có tiền thưởng):
- Khách hàng có mã C48N1001594 và số điện thoại 0905556117:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C48N1001594' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response:
[
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C48N1001594",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C48N1001594",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C48N1001594",
        "display_type": "Tích lũy",
        "accumulated_sales": 226194569,
        "accumulated_points": 2261,
        "membership_tier": "Kim Cương",
        "voucher_code": "2200K",
        "voucher_name": "Hai triệu hai trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 2200000
    }
]

- Khách hàng có mã C75N1000114 và số điện thoại 0986172373:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C75N1000114' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response:
[
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C75N1000114",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C75N1000114",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C75N1000114",
        "display_type": "Trưng bày",
        "accumulated_sales": 47474271,
        "accumulated_points": 474,
        "membership_tier": "Vàng",
        "voucher_code": "300K",
        "voucher_name": "Ba trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 300000
    }
]
- Khách hàng có mã C75N1000655 và số điện thoại 0937782277:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C75N1000655' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response: [
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C75N1000655",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C75N1000655",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C75N1000655",
        "display_type": "Trưng bày",
        "accumulated_sales": 30532820,
        "accumulated_points": 305,
        "membership_tier": "Vàng",
        "voucher_code": "200K",
        "voucher_name": "Hai trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 200000
    }
]

- Khách hàng có mã C77N1000400 và số điện thoại 0987380790:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C77N1000400' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response: [
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C77N1000400",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C77N1000400",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C77N1000400",
        "display_type": "Tích lũy",
        "accumulated_sales": 58501726,
        "accumulated_points": 585,
        "membership_tier": "Vàng",
        "voucher_code": "500K",
        "voucher_name": "Năm trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 500000
    }
]

- Khách hàng có mã C79N1004643 và số điện thoại 0933856863:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C79N1004643' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response: [
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C79N1004643",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C79N1004643",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C79N1004643",
        "display_type": "Trưng bày",
        "accumulated_sales": 40433257,
        "accumulated_points": 404,
        "membership_tier": "Vàng",
        "voucher_code": "300K",
        "voucher_name": "Ba trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 300000
    }
]

- Khách hàng có mã C94N1000225 và C94N1000286 và số điện thoại 0838172900:
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C94N1000225' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response: [
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C94N1000225",
        "display_type": "Trưng bày",
        "accumulated_sales": 36596356,
        "accumulated_points": 365,
        "membership_tier": "Vàng",
        "voucher_code": "200K",
        "voucher_name": "Hai trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 200000
    }
]
CURL: curl --location 'https://dmsapi.hoalinh.io.vn/api/CustomerCampaigns/C94N1000286' \
--header 'accept: application/json' \
--header 'X-API-Key: HLDms_ZaloOrder_SecretKey_2026'
Response: [
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C94N1000286",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "QT34",
        "voucher_name": "Hộp quà tặng Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "GIFT25NAM",
        "campaign_name": "Tặng quà kỷ niệm 25 năm",
        "campaign_period": 1,
        "start_date": "2026-09-25",
        "end_date": "2026-11-30",
        "cust_code": "C94N1000286",
        "display_type": null,
        "accumulated_sales": 0,
        "accumulated_points": 0,
        "membership_tier": null,
        "voucher_code": "TD949",
        "voucher_name": "Lọ Hoa Hoa Linh 25 năm",
        "voucher_type": 2,
        "voucher_value": 1
    },
    {
        "campaign_code": "THTA202601",
        "campaign_name": "Thăng hạng tri ân 2026 - đợt 1",
        "campaign_period": 1,
        "start_date": "2026-01-01",
        "end_date": "2026-07-01",
        "cust_code": "C94N1000286",
        "display_type": "Trưng bày",
        "accumulated_sales": 50348394,
        "accumulated_points": 503,
        "membership_tier": "Vàng",
        "voucher_code": "500K",
        "voucher_name": "Năm trăm nghìn đồng",
        "voucher_type": 1,
        "voucher_value": 500000
    }
]

YÊU CẦU
- Hãy kiểm tra và tìm lý do vì sao lại gặp trường hợp này và đưa ra đề xuất để khắc phục để tình trạng này không diễn ra trong tương lai.
- Gửi câu lệnh query Sql để kiểm tra số tiền khách hàng đổi và số lượng giao dịch đổi quà tương ứng với khách hàng để biết khách hàng nào đang đổi vượt hạn mức để chặn không cho khách hàng thấy được quà trên front end để tránh khách sử dụng (phương án trước mắt).
- Cần cung cấp thông tin gì hay dữ liệu gì thì báo lại để tôi cung cấp.
LƯU Ý ĐẶC BIỆT:
- Chỉ báo cáo lại để tôi kiểm tra và phê duyệt nếu cần phải chỉnh sửa code.
- Việc này rất hệ trọng tới rủi ro phải đền bù tiền.
- Xong hãy cập nhật báo cáo kết quả vào cuối file này.