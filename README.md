# Tạo bản thiết kế Database đầu tiên
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project WebAPI

# Cập nhật cấu trúc vào PostgreSQL
dotnet ef database update --project Infrastructure --startup-project WebAPI



& "$env:USERPROFILE\.dotnet\tools\dotnet-ef" migrations add listDepartmentId --project Infrastructure --startup-project TechAppraisalSystem



& "$env:USERPROFILE\.dotnet\tools\dotnet-ef" database update --project Infrastructure --startup-project TechAppraisalSystem
