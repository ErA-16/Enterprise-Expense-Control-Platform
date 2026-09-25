# 💼 Enterprise Expense Control Platform

> A full-stack enterprise expense management platform that manages the complete lifecycle of employee expenses — from submission and managerial approval to finance processing, payment, notifications, and audit history.

<p align="center">

[![Live Demo](https://img.shields.io/badge/Live%20Demo-Expense%20Platform-success?style=for-the-badge)](https://expense-control-frontend-aedl.onrender.com)
[![Backend API](https://img.shields.io/badge/API-ASP.NET%20Core-blue?style=for-the-badge)](https://expense-control-backend-ile7.onrender.com)

</p>

<p align="center">

![C#](https://img.shields.io/badge/C%23-512BD4?style=flat-square\&logo=csharp\&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square\&logo=dotnet\&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=flat-square\&logo=dotnet\&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=flat-square\&logo=postgresql\&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-512BD4?style=flat-square)
![React](https://img.shields.io/badge/React-20232A?style=flat-square\&logo=react\&logoColor=61DAFB)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square\&logo=docker\&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=flat-square\&logo=jsonwebtokens\&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=flat-square)
![Tests](https://img.shields.io/badge/Tests-xUnit-red?style=flat-square)

</p>

---

## 🌐 Live Application

**Frontend:**
https://expense-control-frontend-aedl.onrender.com

**Backend API:**
https://expense-control-backend-ile7.onrender.com

The application includes demo accounts so you can explore the different role-based workflows.

---

## 🎯 What Is This?

The Enterprise Expense Control Platform is a business workflow application designed to control how employee expenses move through an organization.

Instead of treating expenses as simple CRUD records, the system models a real approval and payment process:

```text
Employee
   │
   ▼
Submit Expense
   │
   ▼
Pending
   │
   ▼
Manager Review
   │
   ├──────────────► Rejected
   │
   ▼
Approved
   │
   ▼
Finance Review
   │
   ├──────────────► Rejected
   │
   ▼
Processing
   │
   ▼
Payment
   │
   ▼
Paid
```

Every major transition is recorded in the audit history.

---

## 👥 Roles

### 👨‍💻 Employee

* Submit expense requests
* Upload receipts/documents
* Track expense status
* View expense history
* Receive notifications
* Use AI assistance to improve expense descriptions

### 👨‍💼 Manager

* View departmental expenses
* Review employee requests
* Approve or reject expenses
* Add review comments
* Monitor department spending
* View audit history

### 💰 Finance

* Review manager-approved expenses
* Approve/reject finance requests
* Process payments
* Select payment methods
* Upload payment proof
* View paid expenses
* View spending reports

### 🛡️ Admin

* Manage departments
* Access organization-level information and reports

---

## ✨ Key Features

* 🔐 JWT authentication
* 👥 Role-based authorization
* 🏢 Department-based access control
* 🧾 Expense request management
* 📎 Receipt/document uploads
* ✅ Multi-stage approval workflow
* 💳 Payment processing workflow
* 🧾 Payment proof uploads
* 📜 Complete audit history
* 🔔 In-app notifications
* ⚡ SignalR infrastructure
* ⏰ Background reminder jobs
* 🤖 AI-assisted expense descriptions
* 📊 Department expense reports
* 🧪 Automated tests
* 🐳 Docker support
* 🗄️ PostgreSQL database
* 🧱 Service/interface architecture

---

## 🔄 Expense Workflow

| Stage             | Responsible Role | Action               |
| ----------------- | ---------------- | -------------------- |
| 📝 Pending        | Employee         | Expense submitted    |
| 🔍 Review         | Manager          | Approve / Reject     |
| 💰 Finance Review | Finance          | Approve / Reject     |
| ⚙️ Processing     | Finance          | Prepare payment      |
| 💳 Paid           | Finance          | Complete payment     |
| 📜 Audit          | Authorized users | View request history |

---

## 🧪 Demo Accounts

> **Password for all demo accounts:** `Demo123`

| Role           | Email                |
| -------------- | -------------------- |
| 👨‍💻 Employee | `employee1@demo.com` |
| 👨‍💻 Employee | `employee2@demo.com` |
| 👨‍💼 Manager  | `manager1@demo.com`  |
| 👨‍💼 Manager  | `manager2@demo.com`  |
| 💰 Finance     | `finance@demo.com`   |

### Recommended Demo Flow

For the full experience:

**1. Login as Employee**

Create an expense request and upload a receipt.

**2. Login as Manager**

Review and approve the request.

**3. Login as Finance**

Review the approved request, process the payment, and upload payment proof.

**4. Return to the request**

View the complete audit trail and notifications.

---

## 🏗️ Architecture

The backend follows a layered architecture with controllers, services, interfaces, DTOs, data access, background services, and supporting infrastructure.

```text
┌─────────────────────┐
│      React UI       │
└──────────┬──────────┘
           │ HTTP / SignalR
           ▼
┌─────────────────────┐
│   ASP.NET Core API  │
├─────────────────────┤
│     Controllers     │
├─────────────────────┤
│      Services       │
├─────────────────────┤
│   Business Rules    │
├─────────────────────┤
│   Entity Framework  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│     PostgreSQL      │
└─────────────────────┘
```

Business logic is kept inside services rather than being placed directly inside controllers.

---

## 🛠️ Tech Stack

### Backend

* **C# / .NET 10**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **PostgreSQL**
* **JWT Authentication**
* **BCrypt**
* **SignalR**
* **Microsoft.Extensions.AI**
* **Google Gemini**

### Frontend

* **React**
* **Vite**
* **Tailwind CSS**
* **React Router**
* **SignalR Client**

### Testing & DevOps

* **xUnit**
* **Moq**
* **Docker**
* **Render**

---

## 📡 API Highlights

```http
POST /api/Auth/login

POST /api/Expense/submit-request

GET /api/Expense/myrequests

GET /api/Expense/manager/pending-requests

POST /api/Expense/manager/{id}/process

GET /api/Expense/finance/pending-requests

POST /api/Expense/finance/{id}/process

POST /api/Expense/finance/{id}/pay

GET /api/Expense/{id}/history

GET /api/Expense/reports/expenses/by-department

POST /api/Ai/assist
```

Authentication and role-based authorization are applied to protected endpoints.

---

## 🤖 AI Integration

The platform includes an AI assistant that helps users improve expense descriptions before submission.

AI is intentionally a **supporting feature**, not the core of the application.

The core system is built around explicit business rules, authorization, workflow states, auditability, and financial operations.

---

## ⏰ Background Processing

A hosted background service monitors pending expense requests.

Requests that remain pending for more than **12 hours** can trigger reminder notifications.

This allows time-based business logic to run independently of normal HTTP requests.

---

## 🧪 Testing

The project includes automated tests covering important controller and service behavior.

Run:

```bash
dotnet test
```

---

## 🚀 Running Locally

### Requirements

* .NET 10 SDK
* PostgreSQL
* Node.js
* npm

### Backend

```bash
git clone https://github.com/ErA-16/Enterprise-Expense-Control-Platform.git

cd Enterprise-Expense-Control-Platform

dotnet restore

dotnet run
```

Configure your environment with:

```text
DefaultConnection
Jwt:Key
Jwt:Issuer
Jwt:Audience
Gemini:ApiKey
```

### Frontend

```bash
cd Expense_Frontend

npm install

npm run dev
```

---

## 🔒 Security

Sensitive values such as:

* Database credentials
* JWT signing keys
* AI API keys

should be provided through environment variables, .NET User Secrets, or deployment-platform secrets.

The demo credentials are intended only for exploring the deployed application.

---

## 📌 Why I Built This

This project was built to move beyond basic CRUD API development and practice designing a system around real business workflows.

Key engineering concepts explored include:

* REST API design
* Authentication & authorization
* Role-based access control
* Business rules
* State transitions
* Service-oriented architecture
* Dependency injection
* Entity Framework Core
* PostgreSQL
* File handling
* Audit trails
* Notifications
* Background processing
* AI integration
* Automated testing
* Docker
* Frontend/API integration
* Deployment

---

## 👨‍💻 Author

**Taiwo**

Built with C#, ASP.NET Core, PostgreSQL, React, and a focus on practical backend/software engineering.
