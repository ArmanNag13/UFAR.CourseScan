# **UFAR CourseScan Backend 📚**

**UFAR CourseScan Backend** is the server-side component of the **UFAR CourseScan** platform developed during the **Project S4** course at **Université Française en Arménie (UFAR)**. It handles the API endpoints, data parsing, and database management that powers the web-based platform.

---

## **Project Description**

The **UFAR CourseScan Backend** automates the extraction and management of course syllabuses by:

- **PDF syllabus upload**
- **Text extraction**
- **Data parsing**
- **Structured storage in a relational database**

This backend service powers the **UFAR CourseScan UI** (frontend), enabling easy access to course details like learning outcomes, assessments, syllabus topics, and more. The system supports multi-language syllabuses in **English**, **Armenian**, and **French**.

---

## **Key Features**

- **API Endpoints**: Handles course data management through secure API endpoints.
- **Data Parsing**: Automatically parses syllabuses from PDF format into structured data.
- **Database Integration**: Utilizes **Microsoft SQL Server** for storing and querying course data.
- **Multi-language Support**: Supports **English**, **Armenian**, and **French** for syllabuses.
- **Efficient PDF Parsing**: Leverages **iTextSharp** or **PdfSharp** for extracting data from PDFs.

---

## **Tools & Technologies**

- **Backend Framework**: **C# (.NET Core)** for API development and business logic.
- **Database**: **Microsoft SQL Server** for structured course data storage.
- **PDF Parsing**: **iTextSharp** and **PdfSharp** for parsing PDF syllabuses.
- **Version Control**: **Git** (with **GitHub**) for managing source code.
- **Project Management**: **Asana** for tracking progress and task management.

---

## **Installation & Setup**

Follow these steps to set up the UFAR CourseScan Backend on your local machine:

### **Prerequisites**
Ensure you have the following installed:
- **.NET SDK**: [Download here](https://dotnet.microsoft.com/)
- **Visual Studio**: [Download here](https://visualstudio.microsoft.com/)
- **SQL Server Management Studio (SSMS)**: [Download here](https://aka.ms/ssmsfullsetup)

### **Setup Instructions**
1. **Clone the repository:**
    ```bash
    git clone https://github.com/ArmanNag13/UFAR.CourseScan.Backend.git
    cd UFAR.CourseScan.Backend
    ```

2. **Open the solution in Visual Studio**:
    - Open the `.sln` solution file in **Visual Studio**.
    - Restore NuGet packages by right-clicking the solution and selecting **Restore NuGet Packages**.

3. **Set up the database**:
    - Ensure **SQL Server** is running on your machine.
    - Create a database called `UFARCourseScan` (or connect to an existing database using the connection string in `appsettings.json`).
    - Run migrations to set up the database schema:
    ```bash
    dotnet ef database update
    ```

4. **Run the application**:
    - Press **F5** or click **Start** in Visual Studio to run the backend.
    - The API will be available at `http://localhost:5000`.

---

## **Related Repositories**

- [UFAR CourseScan UI](https://github.com/ArmanNag13/UFAR.CourseScan.UI) (Frontend)

---

## **Team Members & Roles**

| Name               | Role                            |
|--------------------|----------------------------------|
| **Vahe Mirzoyan**   | Project Manager & Tester         |
| **Arman Nagdalyan** | Parsing Expert                   |
| **Arsen Martirosyan** | Frontend Developer               |
| **Artur Babayan**   | Database Developer               |
| **Artur Gevorgyan** | Data Extraction Specialist       |

---

## **Contributing**

If you'd like to contribute to **UFAR CourseScan Backend**, feel free to fork the repository and submit a pull request. Contributions are welcome, including:
- Bug fixes
- New features
- Documentation updates

Please ensure that any changes align with the project's goals and provide proper tests for any new features.

---

## **License**

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

---

## **Acknowledgements**

- **Université Française en Arménie (UFAR)** for supporting this project as part of the Project S4 course.
- **iTextSharp** and **PdfSharp** for their powerful PDF parsing capabilities.
- The **.NET Core** and **Blazor** communities for their robust frameworks.
