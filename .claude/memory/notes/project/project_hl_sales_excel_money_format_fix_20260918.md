# Hoa Linh Sales Excel — trailing decimal separator fix (2026-09-18)

- User confirmed download works, requested money output `600,000.` become `600,000`.
- Updated HlSalesExportAppService Excel number format `#,##0.##` → `#,##0` on PointHistory column 10 (Giá trị, both transaction/batch exports), GiftExchanges column 9 (Số tiền), Orders column 5 (Thành tiền).
- Thousands grouping remains; displayed money has no decimal part. Underlying numeric values are preserved, not converted to text.
- Added formatted-output assertions to existing three workbook export tests, reading back exported XLSX: `600,000`, `500,000`, `900,000` with invariant culture.
- Validation: 14 targeted Application tests pass; git diff --check pass. No migration/database write; live Excel client not inspected in this session. Rebuild/restart host and export new files to receive formatting change.
- Branch dev; agent did not commit/deploy. Other existing Sales JS/log/memory changes preserved.
