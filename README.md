# nexgensales
A standalone .NET desktop app for sales tracking, reporting, and prediction.

## Folder structure

- [Commands/](Commands/): Command handlers, CLI utilities, and scripts invoked by the app.
- [Models/](Models/): Domain models, DTOs, validation logic, and persistence schemas.
- [ViewModels/](ViewModels/): View-models or presentation-layer models that adapt `Models` for UI binding.
- [Services/](Services/): Business logic, API clients, data access services, and orchestrators.
- [Services/Data/](Services/Data/): Data-specific services such as repositories, storage adapters, seeding, and migrations.
- [UserComponents/](UserComponents/): Reusable UI components, controls, and widgets used by views.
- [Views/](Views/): Screens, pages, and templates that compose `UserComponents` and bind to `ViewModels`.

## Getting started

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

## Branching & merging

The repo includes a Husky pre-commit hook that blocks commits directly to `main`. Use feature branches and merge changes into `main` only via Pull Requests (PRs) on Github

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


