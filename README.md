# PROG6212_POE_Part3

# Contract Monthly Claim System (CMCS)

A comprehensive web-based application for managing monthly contract claims in an academic institution. The system facilitates claim submission, approval workflows, reporting, and document management for lecturers, coordinators, managers, and HR personnel.

## 📺 Demo & Presentation

**Watch the full presentation:** [YouTube Video](https://youtu.be/Wpu9KNETWH8)

## 🎯 Overview

The Contract Monthly Claim System (CMCS) is an ASP.NET Core MVC application designed to streamline the monthly claim submission and approval process for contract lecturers. The system provides role-based access control, automated calculations, document management, and comprehensive reporting capabilities.

## ✨ Key Features

### For Lecturers
- **Secure Login**: Session-based authentication for lecturers
- **Auto-Populated Forms**: Name, surname, and hourly rate automatically populated from HR data
- **Auto-Calculation**: Total amount calculated based on hours worked and hourly rate
- **Monthly Hours Validation**: Maximum 180 hours per month validation
- **Claim Tracking**: View and track the status of submitted claims
- **Document Upload**: Upload supporting documents (PDF, DOCX, XLSX, images) with claims

### For Programme Coordinators
- **Dashboard View**: Overview of all claims with statistics
- **Claim Review**: Review and approve/reject claims
- **Recent Activity**: Track recent claim submissions

### For Academic Managers
- **Dashboard View**: Comprehensive view of all claims and approvals
- **Approval Management**: Review and manage claim approvals
- **Latest Approvals**: Monitor recently approved claims

### For HR Personnel
- **Report Generation**: Generate detailed claim reports
- **Export Functionality**: Export reports to various formats
- **User Management**: Manage user accounts and roles
- **Data Analytics**: View claim statistics and trends

### For Administrators
- **User Management**: Create, edit, and delete users
- **Role Management**: Manage system roles and permissions
- **Claim Management**: Full CRUD operations on claims
- **Document Management**: Upload and manage supporting documents
- **System Configuration**: Configure system settings

## 🏗️ System Architecture

### Technology Stack
- **Framework**: ASP.NET Core MVC 9.0
- **Database**: SQL Server (with JSON file fallback for data persistence)
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, Font Awesome
- **Authentication**: Session-based authentication
- **Testing**: xUnit, Moq

### Data Persistence
The system uses a hybrid approach:
- **Primary**: SQL Server database (Entity Framework Core)
- **Fallback**: JSON file storage (`Data/*.json` files)
  - `users.json` - User data
  - `claims.json` - Claim data
  - `approvals.json` - Approval records
  - `roles.json` - Role definitions
  - `documents.json` - Document metadata

## 👥 User Roles

| Role | RoleID | Description |
|------|--------|-------------|
| **Lecturer** | 1 | Contract lecturers who submit monthly claims |
| **Coordinator** | 2 | Programme coordinators who review claims |
| **Manager** | 3 | Academic managers who approve claims |
| **Administrator** | 5 | System administrators with full access |

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 or later
- SQL Server (optional - system works with JSON files)
- Modern web browser (Chrome, Firefox, Edge, Safari)

### Installation

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd "Contract Monthly Claim System (CMCS)"
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Configure Database (Optional)**
   - Update `appsettings.json` with your SQL Server connection string:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=YOUR_SERVER;Database=CMCS;Trusted_Connection=True;TrustServerCertificate=True;"
       }
     }
     ```
   - Or use the provided `CMCS.sql` script to create tables manually
   - **Note**: The system will automatically create tables on first run if using EF Core

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run the Application**
   ```bash
   cd "Contract Monthly Claim System (CMCS)"
   dotnet run
   ```

6. **Access the Application**
   - Open your browser and navigate to: `https://localhost:5001` or `http://localhost:5000`
   - The application will be available at the port shown in the console

### Initial Setup

1. **Create Sample Data** (if using JSON files):
   - Sample users are automatically created in `Data/users.json`
   - Sample roles are in `Data/roles.json`

2. **Default Login Credentials**:
   - **Lecturer**: Use any user with `RoleID = 1` (e.g., `khumo.t@university.edu`)
   - **Coordinator**: Use any user with `RoleID = 2`
   - **Manager**: Use any user with `RoleID = 3`
   - **Admin**: Use any user with `RoleID = 5`

## 📖 Usage Guide

### For Lecturers

1. **Login**
   - Navigate to "Lecturer Login" from the main menu
   - Enter your email and password
   - Click "Login"

2. **Submit a Claim**
   - Click "Submit New Claim" from the dashboard
   - Form auto-populates with your details
   - Enter hours worked (max 180 per month)
   - Upload supporting documents (optional)
   - Click "Submit Claim"

3. **Track Claims**
   - Click "My Claims" to view all your submitted claims
   - View claim status, approval details, and notes

### For Coordinators

1. **Login**
   - Click "Coordinator" from the main menu
   - Enter your email and password
   - Click "Login"

2. **Review Claims**
   - View all claims in the dashboard
   - Click on a claim to view details
   - Approve or reject claims with notes

### For Managers

1. **Login**
   - Click "Manager" from the main menu
   - Enter your email and password
   - Click "Login"

2. **Approve Claims**
   - View all claims and approvals in the dashboard
   - Review coordinator recommendations
   - Final approval/rejection of claims

### For HR

1. **Generate Reports**
   - Navigate to HR → Generate Report
   - Select date range and filters
   - Click "Generate Report"
   - Export to desired format

2. **User Management**
   - Navigate to User Management
   - Create, edit, or delete users
   - Assign roles and hourly rates

## 🧪 Testing

### Running Unit Tests

#### Method 1: Visual Studio Test Explorer
1. Open **Test Explorer**: `Test` → `Test Explorer` (or `Ctrl+E, T`)
2. Build the solution: `Build` → `Build Solution` (`Ctrl+Shift+B`)
3. Run all tests: Click `Run All Tests` (or `Ctrl+R, A`)
4. View results: Green checkmark = Passed, Red X = Failed

#### Method 2: Command Line
```bash
cd "(CMCS).UnitTest"
dotnet test
```

For detailed output:
```bash
dotnet test --verbosity detailed
```

### Test Coverage

The test suite includes:

- **Controller Tests** (5 tests per controller):
  - `ClaimControllerTests` - Claim CRUD operations
  - `UserControllerTests` - User management
  - `ApprovalControllerTests` - Approval workflows
  - `RoleControllerTest` - Role management

- **Model Validation Tests** (7 tests):
  - User model validation
  - Claim model validation
  - Role model validation

- **Business Logic Tests** (8 tests):
  - Status properties
  - Total amount calculations
  - User full name
  - Approval creation

- **Error Handling Tests** (11 tests):
  - Invalid ID handling
  - Null property handling
  - Empty status handling

- **Integration Tests** (7 tests):
  - End-to-end workflows
  - Data consistency
  - Navigation flow

- **Comprehensive Test Suite** (13 tests):
  - Complete system workflows
  - Cross-module integration

**Total: 65+ unit tests**

## 📁 Project Structure

```
Contract Monthly Claim System (CMCS)/
├── Contract Monthly Claim System (CMCS)/
│   ├── Controllers/          # MVC Controllers
│   │   ├── AdminController.cs
│   │   ├── ApprovalController.cs
│   │   ├── ClaimController.cs
│   │   ├── CoordinatorController.cs
│   │   ├── HRController.cs
│   │   ├── LecturerController.cs
│   │   ├── ManagerController.cs
│   │   ├── RoleController.cs
│   │   ├── SupportingDocumentController.cs
│   │   ├── TrackingController.cs
│   │   └── UserController.cs
│   ├── Models/               # Data Models
│   │   ├── Approval.cs
│   │   ├── Claim.cs
│   │   ├── ClaimCreateViewModel.cs
│   │   ├── Role.cs
│   │   ├── SupportingDocument.cs
│   │   └── User.cs
│   ├── Views/                # Razor Views
│   │   ├── Admin/
│   │   ├── Approval/
│   │   ├── Claim/
│   │   ├── Coordinator/
│   │   ├── HR/
│   │   ├── Lecturer/
│   │   ├── Manager/
│   │   ├── Role/
│   │   ├── SupportingDocument/
│   │   ├── Tracking/
│   │   └── User/
│   ├── Data/                 # JSON Data Files
│   │   ├── approvals.json
│   │   ├── claims.json
│   │   ├── documents.json
│   │   ├── roles.json
│   │   ├── users.json
│   │   └── CmcsDbContext.cs
│   ├── wwwroot/             # Static Files
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── uploads/
│   ├── Program.cs           # Application Entry Point
│   └── appsettings.json     # Configuration
├── (CMCS).UnitTest/         # Unit Tests
│   ├── ApprovalControllerTests.cs
│   ├── BusinessLogicTests.cs
│   ├── ClaimControllerTests.cs
│   ├── ComprehensiveTestSuite.cs
│   ├── ErrorHandlingTests.cs
│   ├── IntergrationTests.cs
│   ├── ModelValidationTest.cs
│   ├── RoleControllerTest.cs
│   └── UserControllerTests.cs
├── CMCS.sql                 # Database Script
└── README.md                # This file
```

## 🔒 Security Features

- **Session Management**: Secure session-based authentication
- **Anti-Forgery Tokens**: CSRF protection on all forms
- **Role-Based Access Control**: Restricted access based on user roles
- **Input Validation**: Server-side and client-side validation
- **File Upload Security**: File type and size validation
- **Password Hashing**: Secure password storage (ready for implementation)

## 📊 Key Business Rules

1. **Monthly Hours Limit**: Maximum 180 hours per month per lecturer
2. **Claim Status Workflow**: Pending → Approved/Rejected → Processing → Completed
3. **User-Claim Relationship**: Every claim must be linked to a valid user
4. **Approval Requirement**: Only approved claims appear in the approvals table
5. **Document Support**: Claims can have multiple supporting documents
6. **Auto-Calculation**: Total amount = Hours Worked × Hourly Rate

## 🐛 Troubleshooting

### Tests Not Showing in Test Explorer
1. Rebuild the solution: `Build` → `Rebuild Solution`
2. Restore NuGet packages: Right-click solution → `Restore NuGet Packages`
3. Refresh Test Explorer: Click the refresh icon

### Database Connection Issues
- The system will automatically fall back to JSON file storage if the database is unavailable
- Check `appsettings.json` for correct connection string
- Ensure SQL Server is running (if using database)

### Session Issues
- Clear browser cookies and cache
- Ensure session middleware is enabled in `Program.cs`
- Check session timeout settings (default: 30 minutes)

### File Upload Issues
- Check file size (max 10MB per file)
- Check file extension (allowed: .pdf, .docx, .xlsx, .doc, .xls, .jpg, .jpeg, .png)
- Ensure `wwwroot/uploads/` directory exists and has write permissions

## 🔄 Version History

### Part 3 Improvements
- ✅ Lecturer View with auto-populated forms
- ✅ Session-based authentication for all roles
- ✅ Document upload integration
- ✅ Enhanced UI with better visibility and contrast
- ✅ User management with delete functionality
- ✅ Comprehensive unit test suite (65+ tests)
- ✅ Improved data consistency across reports
- ✅ Role-based dashboards for Coordinator and Manager
- ✅ Enhanced claim tracking and status management

### Part 2 Features
- Basic claim submission
- User and role management
- Approval workflows
- Report generation
- Document management

## 📝 License

This project is developed for academic purposes as part of PROG6212 - Programming 2B.

## 👤 Author

**Student Number**: ST10446648  
**Course**: PROG6212 - Programming 2B  
**Institution**: IIE MSA

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the responsive UI framework
- Font Awesome for the icon library
- xUnit team for the testing framework

## 📞 Support

For issues, questions, or contributions, please refer to the project documentation or contact the development team.

---

**Last Updated**: 2025  
**Status**: Production Ready ✅

