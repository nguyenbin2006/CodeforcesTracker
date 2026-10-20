# 🚀 Codeforces Rating Tracker

A smart web application built with **ASP.NET Core MVC** that helps competitive programmers track their Codeforces submissions, analyze algorithmic weaknesses, and get personalized problem suggestions.

## ✨ Features

*   **🔄 Automated Data Sync:** Fetches user submissions from the official Codeforces API (`user.status`) and securely stores them in a local SQL Server database, avoiding duplicate records.
*   **📊 Weakness Analysis:** Groups submissions by algorithm tags (e.g., `dp`, `math`, `graphs`) and calculates the failure rate using C# LINQ to pinpoint exactly where you need improvement.
*   **💡 Smart Suggestions:** Integrates with the `problemset.problems` API to cross-reference your solved problems and suggest 5 tailored, unsolved problems matching your weakest algorithm tag.

## 🛠️ Tech Stack

*   **Backend:** C# 12, .NET 8.0, ASP.NET Core MVC
*   **Database:** Microsoft SQL Server, Entity Framework Core (Code-First)
*   **Frontend:** HTML5, CSS3, Bootstrap 5
*   **Data Processing:** Newtonsoft.Json, HttpClient

## 🚀 Getting Started

### Prerequisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
*   Microsoft SQL Server
*   Visual Studio 2022 (Recommended)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/nguyenbin2006/CodeforcesTracker.git](https://github.com/nguyenbin2006/CodeforcesTracker.git)
    cd CodeforcesTracker
    ```

2.  **Configure the Database:**
    Open `appsettings.json` and update the `DefaultConnection` string with your local SQL Server credentials.

3.  **Apply Migrations:**
    Open the Package Manager Console in Visual Studio and run:
    ```powershell
    Update-Database
    ```

4.  **Run the App:**
    Press `F5` in Visual Studio to launch the application. Enter your Codeforces handle on the home page to start syncing data!

## 📸 Screenshots
<img width="1519" height="798" alt="image" src="https://github.com/user-attachments/assets/f9a3841d-4148-4ef0-a722-29c48310e4c9" />
<img width="1535" height="815" alt="image" src="https://github.com/user-attachments/assets/d05b192a-bdc2-4613-8b66-8c969a9f7be5" />
<img width="1528" height="819" alt="image" src="https://github.com/user-attachments/assets/fde60967-c2de-474c-b447-15ed17b58999" />




---
*Developed with ❤️ by Nguyen Tan Bin.*
