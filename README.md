# NexGenSales

A standalone .NET desktop app for sales tracking, reporting, and prediction.

## Features & Technical Specifications

**NexGenSales** is designed to handle full-lifecycle sales data management, from raw file ingestion to predictive analytics. It uses a modern C# architecture to provide actionable insights for small to medium-sized business owners without requiring an external database server.

### 1. Data Ingestion & Schema

The system supports batch importing of multiple Excel files simultaneously, separating daily sales from monthly expenses. It enforces a strict schema for incoming data:

* **Sales Records:** Requires `Date_Time`, `Item_ID`, `Supplier_ID`, `Quantity_Sold`, `Unit_Purchase_Cost`, `Unit_Sale_Price`, `Allowed_Discount`, `Net_Revenue`, and `Current_Stock`.
* **Expense Records:** Requires `Date_Recorded`, `Expense_Category`, `Specific_Type`, `Amount`, and `Asset_ID`.

### 2. Time-Series Filtering

The analysis engine dynamically anchors queries to the latest uploaded data record. Users can filter dashboard views using standard operational windows:

* **Sales Intervals:** 1 week, 1 month, 3 months, 6 months, 9 months, and 1 year.
* **Expense Intervals:** 1 to *n* months.

### 3. Business Intelligence Analytics

The application computes key performance indicators (KPIs) across sales and expenses:

* **Sales Analytics:**
  * **Supplier Profitability:** Calculates cost-to-profit ratios and automatically flags underperforming/low-margin suppliers.
  * **Item Velocity:** Ranks inventory by sales volume to highlight best-sellers and slow-movers.
  * **Revenue Contribution:** Calculates average daily revenue generation per item.
  * **Trend Analysis:** Renders day-wise comparative total revenue charts, utilizing dynamic daily groupings (e.g., `MMM dd`) for clarity over long periods.
  * **Discount Optimization:** Computes the most effective discount thresholds by calculating **Average Profit per Transaction** (normalizing zero-discount volumes) and generates a dynamic UI insight string.

* **Expense Analytics:**
  * **Anomaly Detection:** Flags unusual spikes across *all* expense categories using a dynamic standard deviation threshold (`Average + 1.5 * StdDev`) and a > 5000 absolute minimum to isolate true outliers.
  * **Category Breakdown:** Groups total expenses proportionally across categories.
  * **Daily Expense Trend:** Maps out timelines of organizational expenses.
  * **Asset Maintenance Costs:** Accumulates maintenance expenditures mapped to physical assets.
  * **Top Specific Expenses:** Isolates and ranks the top 5 highest expense line-items.



### 4. Visualization & UI (Dashboard)

* **Charting:** Integrated dynamic charting using **LiveCharts2** for visualizing net revenue, sales trends, and expenses with fluid animations.
* **Data Filtering:** Real-time Date-Range filters and UI DataGrids to analyze specific operational windows.

### 5. Exporting & Reporting Module

* **Executive Reports:** Programmatic, high-performance PDF generation using **QuestPDF** for professional, easily distributable sales summaries.
* **Data Export:** Capability to export filtered database records back out to `.xlsx` format for external accounting use.

### 6. Technical Architecture

* **Framework:** .NET WPF (Windows Presentation Foundation) using the MVVM (Model-View-ViewModel) design pattern.
* **Data Persistence:** Local SQLite implementation using the Repository Pattern.
* **Data Access (No ORM):** Built using pure ADO.NET (`Microsoft.Data.Sqlite`) with raw SQL queries and custom row mappers. **No ORM is used**, ensuring maximum execution speed and absolute control over memory allocation and query optimization.
* **Automated Migrations:** A custom `DatabaseMigrationService` utilizes `PRAGMA user_version` to safely auto-generate tables and handle schema updates on application startup.

---

## Getting Started

Prerequisites: Install the .NET SDK and Node.js (Node/npm is only required if you want to use Husky hooks).

Basic steps:

1. Clone the repository:

```bash
git clone <repo-url>
cd nexgensales

```

2. Restore and build the .NET project (run from the folder that contains the main `.csproj`/solution):

```bash
dotnet restore
dotnet build
dotnet run

```

3. Husky (Git hooks) —  this repo includes a `.husky/pre-commit` hook that blocks commits to the `main` branch. Prefer installing Husky using the .NET global tool:

```bash
dotnet new tool-manifest 
dotnet tool install
dotnet husky install

```

---

## Data Import Manifest

To ensure accurate reporting, prediction, and dashboard analytics, all sales and expenses data must be formatted correctly before uploading to NexGenSales. Please follow the guidelines below when preparing your `.xlsx` or `.csv` files.

### General Rules for All Uploads

1. **Single-Day Rule:** Every uploaded file must contain records for **one single calendar date only**.
* *Allowed:* Multiple transactions occurring at different times on the same day (e.g., 09:00:00 and 14:30:00 on 2026-06-01).
* *Not Allowed:* Mixing June 1st and June 2nd data in the same file.


2. **Column Headers:** The first row of your spreadsheet must contain the exact column headers listed below.
3. **No Empty Rows:** Ensure there are no completely blank rows interspersed within your data.

### 1. Sales Records

**File Naming Convention (Recommended):** `Sales_YYYYMMDD.xlsx` (e.g., `Sales_20260601.xlsx`)

Your sales file must contain the following columns. Order does not matter, but the header names must match exactly:

| Required Header | Data Type | Description & Example |
| --- | --- | --- |
| **Date_Time** | Date/Time | The exact time of the sale. *Format: YYYY-MM-DD HH:MM:SS* (e.g., `2026-06-01 14:30:00`) |
| **Item_ID** | Text | The unique identifier for the product sold. (e.g., `ITM-992`) |
| **Supplier_ID** | Text | The identifier for the supplier who provided the item. (e.g., `SUP-04`) |
| **Quantity_Sold** | Number | Total units sold in this transaction. (e.g., `5`) |
| **Unit_Purchase_Cost** | Currency | The cost the business paid for a single unit. (e.g., `12.50`) |
| **Unit_Sale_Price** | Currency | The price the customer paid for a single unit. (e.g., `20.00`) |
| **Allowed_Discount** | Currency | Any discount applied to the total transaction. (e.g., `2.00` or `0`) |
| **Net_Revenue** | Currency | The final revenue collected. *(Quantity * Sale Price) - Discounts*. |
| **Current_Stock** | Number | The remaining inventory of this item *after* the sale. (e.g., `45`) |

### 2. Expenses Records

**File Naming Convention (Recommended):** `Expenses_YYYYMMDD.xlsx` (e.g., `Expenses_20260601.xlsx`)

Your expenses file must contain the following columns. Order does not matter, but the header names must match exactly:

| Required Header | Data Type | Description & Example |
| --- | --- | --- |
| **Date_Recorded** | Date/Time | The exact time the expense occurred or was logged. *Format: YYYY-MM-DD HH:MM:SS* (e.g., `2026-06-01 09:15:00`) |
| **Expense_Category** | Text | The broad category of the expense. (e.g., `Utilities`, `Rent`, `Maintenance`, `Payroll`) |
| **Specific_Type** | Text | A detailed description of the expense. (e.g., `Electricity Bill`, `POS Terminal Repair`) |
| **Amount** | Currency | The total cost of the expense. (e.g., `245.50`) |
| **Asset_ID** | Text | *(Optional)* The ID of the physical asset being repaired/maintained. Leave blank if not applicable. (e.g., `AST-1042`) |

---

## Application Data & Logging

The application utilizes compiler directives (`#if DEBUG`) to dynamically manage file paths for the local SQLite database and system logs. This provides a convenient developer experience while ensuring the application remains crash-resistant and permission-safe in production.

### Build Configurations

* **Development (`Debug` Mode)**
* **Triggered by:** `dotnet run` (or pressing F5 in Visual Studio).
* **Behavior:** The `Database/` and `Logs/` folders are generated directly inside the main project root (`../../..`). This allows developers to easily inspect the SQLite database and log files without hunting through hidden system folders.


* **Production (`Release` Mode)**
* **Triggered by:** `dotnet run -c Release` (or building via `dotnet publish -c Release`).
* **Behavior:** The app dynamically shifts to production-safe paths. The database is generated safely next to the compiled executable, and logs are routed to the user's hidden AppData folder (`%LOCALAPPDATA%\NextGenSales\Logs`).



### Log Files

The application features a global logging infrastructure that runs passively in the background:

* **`console.log`**: A custom dual-writer intercepts all `Console.WriteLine()` calls. It automatically injects timestamps and writes the output to this physical file, while still pushing the text to your IDE's debug console.
* **`error.log`**: A global exception handler catches any unhandled crashes (UI thread, background tasks, or AppDomain). Instead of failing silently, the app writes the complete stack trace to this file and presents a graceful UI warning before closing.

---

## Folder Structure

Top-level project layout (common files and folders you will see in the repo):

* `App.xaml` / `App.xaml.cs` — WPF application entry
* `nexgensales.sln`, `nexgensales.csproj` — solution and project file
* `README.md` — this document
* `LICENSE`

Main source folders:

* `Core/` : Core infrastructure, configuration, utilities, and factories
* `Models/` : Domain models, DTOs, validation logic, and persistence schemas
* `Models/Enums/` : Enum definitions used for mapping/imports


* `ViewModels/` : View-models used for UI binding and presentation logic
* `Views/` : XAML views and their code-behind files
* `UserComponents/` : Reusable UI components and controls
* `Services/` : Business logic, API clients, migrations, and orchestration
* `Services/Data/` : Repositories, SQLite adapters, and mapping abstractions
* `Services/Data/Repository/` : Repository abstractions and concrete implementations
* `Services/Data/Mapper/` : Data mapping operations


* `assets/` : Static assets, images, and resource files

Build artifacts and runtime folders (generated)

* `bin/` : build outputs (Debug/Release)
* `obj/` : intermediate build files
* `Database/` : local SQLite files when running in Debug (generated at runtime)
* `Logs/` : `console.log` and `error.log` when running in Debug (generated at runtime)
* `Reports/` : Exported PDF reports (generated at runtime)

**Folder Structure Overview:**

```text
nexgensales/
├─ App.xaml
├─ nexgensales.sln
├─ nexgensales.csproj
├─ README.md
├─ LICENSE
├─ assets/
│  └─ home/
├─ Core/
├─ Models/
│  ├─ SalesRecord.cs
│  ├─ ExpensesRecord.cs
│  ├─ RecordMetadata.cs
│  ├─ ExpenseAnalyticsResult.cs
│  └─ Enums/
├─ ViewModels/
├─ Views/
├─ UserComponents/
├─ Services/
│  ├─ DatabaseMigrationService.cs
│  └─ Data/
│     ├─ Repository/
│     └─ Mapper/
├─ Database/        <-- created in Debug runs (local SQLite files)
│  └─ app.db
├─ Logs/            <-- created in Debug runs (console.log, error.log)
│  ├─ console.log
│  └─ error.log
├─ Reports/         <-- exported PDF reports
└─ bin/
     └─ Debug/net10.0-windows/

```

Where logs and DB are written depending on build mode:

* **Debug (development):** `Database/` and `Logs/` are created inside the project root (as shown above) to make inspection easy.
* **Release (production):** files are created next to the compiled executable and/or under `C:\Users\<Current User>\AppData\Local\NextGenSales\Logs` for user-safe storage (see `App.xaml.cs`).

---

## Branching & Merging

The repo includes a Husky pre-commit hook that blocks commits directly to `main`. Use feature branches and merge changes into `main` only via Pull Requests (PRs) on Github.

Create a feature branch and push:

```bash
# create and switch to a new branch
git switch -c feature/your-descriptive-name

# stage, commit, and push
git add .
git commit -m "Add short description of changes"
git push origin feature/your-descriptive-name

```

Open a Pull Request from `feature/your-descriptive-name` into `main`.

Do not merge into `main` using local `git merge` + `git push`; always use the PR route so checks, reviews, and hooks run in the expected environment.
