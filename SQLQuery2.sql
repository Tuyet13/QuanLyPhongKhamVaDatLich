USE datlichvaquanly;
GO

/*=========================*
* 1. CHUYÊN KHOA
*=========================*/
INSERT INTO Specialty (SpecialtyName)
VALUES (N'Nội Tổng Quát');
GO

/*=========================*
* 2. USER
*=========================*/
INSERT INTO [User]
(Username, Password, Role, Email, IsActive)
VALUES
(N'doctor1', N'123456', N'Doctor', N'doctor1@gmail.com', 1),
(N'patient1', N'123456', N'Patient', N'patient1@gmail.com', 1),
(N'patient2', N'123456', N'Patient', N'patient2@gmail.com', 1);
GO

/*=========================*
* 3. LẤY USER ID (Thêm ORDER BY DESC để lấy đúng tài khoản vừa tạo)
*=========================*/
DECLARE @DoctorUserId INT =
(SELECT TOP 1 UserId FROM [User] WHERE Username='doctor1' ORDER BY UserId DESC);

DECLARE @P1 INT =
(SELECT TOP 1 UserId FROM [User] WHERE Username='patient1' ORDER BY UserId DESC);

DECLARE @P2 INT =
(SELECT TOP 1 UserId FROM [User] WHERE Username='patient2' ORDER BY UserId DESC);


/*=========================*
* 4. DOCTOR (Sửa gán cứng SpecialtyId thành câu SELECT động)
*=========================*/
DECLARE @SpecId INT = (SELECT TOP 1 SpecialtyId FROM Specialty WHERE SpecialtyName = N'Nội Tổng Quát' ORDER BY SpecialtyId DESC);

INSERT INTO Doctor (FullName, SpecialtyId, Phone, UserId, HireDate, BaseSalary, Rating)
VALUES (N'Nguyễn Hoàng Nam', @SpecId, N'0912345678', @DoctorUserId, GETDATE(), 500000, 5.0);

/*=========================*
* 5. PATIENT
*=========================*/
INSERT INTO Patient
(FullName, Gender, BirthDate, Phone, Address, Email, UserId, IsActive)
VALUES
(N'Lê Văn An', N'Nam', '2000-05-15', N'0901111111', N'Hà Nội', N'patient1@gmail.com', @P1, 1),
(N'Phạm Thị Lan', N'Nữ', '2001-10-20', N'0902222222', N'Hà Nội', N'patient2@gmail.com', @P2, 1);

/*=========================*
* 6. MEDICINE
*=========================*/
INSERT INTO Medicine (MedicineName, Unit, UnitPrice, Description)
VALUES
(N'Paracetamol 500mg', N'Viên', 1500, N'Hạ sốt'),
(N'Amoxicillin 500mg', N'Viên', 3000, N'Kháng sinh');


/*=========================*
* 7. Lịch hẹn (Sửa PatientId lấy đúng ID từ bảng Patient thay vì lấy UserId)
*=========================*/
DECLARE @ActualDocId INT = (SELECT TOP 1 DoctorId FROM Doctor ORDER BY DoctorId DESC);
DECLARE @ActualPat1Id INT = (SELECT TOP 1 PatientId FROM Patient WHERE UserId = @P1 ORDER BY PatientId DESC);
DECLARE @ActualPat2Id INT = (SELECT TOP 1 PatientId FROM Patient WHERE UserId = @P2 ORDER BY PatientId DESC);

INSERT INTO Appointment (PatientId, DoctorId, AppointmentDate, Status, Price)
VALUES
(@ActualPat1Id, @ActualDocId, GETDATE(), N'Chờ khám', 150000),
(@ActualPat2Id, @ActualDocId, DATEADD(HOUR,2,GETDATE()), N'Chờ khám', 150000);


/*=========================*
* 8. LẤY Lịch hẹn ID
*=========================*/
DECLARE @AppointmentId INT =
(SELECT TOP 1 AppointmentId FROM Appointment ORDER BY AppointmentId DESC);


/*=========================*
* 9. Bệnh án
*=========================*/
INSERT INTO MedicalRecord (AppointmentId, Symptom, Diagnosis, Note, PrescriptionSummary, RecordDate)
VALUES (@AppointmentId, N'Sốt cao', N'Cảm cúm', N'Nghỉ ngơi', N'Paracetamol', GETDATE());


/*=========================*
* 10. đơn thuốc
*=========================*/
-- Thay vì SCOPE_IDENTITY() dễ lỗi vùng nhớ, ta SELECT trực tiếp RecordId mới nhất
DECLARE @RecordId INT = (SELECT TOP 1 RecordId FROM MedicalRecord ORDER BY RecordId DESC);

INSERT INTO Prescription (RecordId, MedicineName, Quantity, Dosage, Instruction)
VALUES (@RecordId, N'Paracetamol 500mg', 10, N'2 viên/ngày', N'Uống sau ăn');


/*=========================*
* 11. Chi tiết đơn thuốc (Sửa gán cứng MedicineId từ số 1 thành lấy ID động)
*=========================*/
DECLARE @MedId INT = (SELECT TOP 1 MedicineId FROM Medicine WHERE MedicineName = N'Paracetamol 500mg' ORDER BY MedicineId DESC);

INSERT INTO PrescriptionDetail (MedicalRecordId, MedicineId, Quantity, Dosage)
VALUES (@RecordId, @MedId, 10, N'2 viên/ngày');
/*=========================*
* 12. Đơn xin nghỉ phép
*=========================*/
INSERT INTO LeaveRequest
(UserId, LeaveDate, Status, Reason)
VALUES
(@DoctorUserId,
DATEADD(DAY,5,GETDATE()),
N'Pending',
N'Nghỉ việc gia đình');

/*=========================*
* 13. CHECK DATA
*=========================*/
SELECT * FROM [User];
SELECT * FROM Doctor;
SELECT * FROM Patient;
SELECT * FROM Medicine;
SELECT * FROM Appointment;
SELECT * FROM MedicalRecord;
SELECT * FROM Prescription;
SELECT * FROM PrescriptionDetail;
SELECT * FROM LeaveRequest;

