# How to Run Unit Tests

## Method 1: Visual Studio Test Explorer (Recommended)

1. **Open Test Explorer:**
   - Go to `Test` → `Test Explorer` (or press `Ctrl+E, T`)
   - Or go to `View` → `Test Explorer`

2. **Build the Solution:**
   - Right-click on the solution in Solution Explorer
   - Select `Build Solution` (or press `Ctrl+Shift+B`)

3. **Run All Tests:**
   - In Test Explorer, click `Run All Tests` (or press `Ctrl+R, A`)
   - You'll see all tests listed with pass/fail status

4. **Run Individual Tests:**
   - Right-click on any test and select `Run`
   - Or double-click a test to see its details

5. **View Results:**
   - Green checkmark = Passed
   - Red X = Failed
   - Yellow circle = Not run
   - Click on a test to see detailed output

## Method 2: Command Line (dotnet test)

1. **Open Terminal/Command Prompt:**
   - In Visual Studio: `View` → `Terminal`
   - Or open PowerShell/Command Prompt

2. **Navigate to Test Project:**
   ```powershell
   cd "(CMCS).UnitTest"
   ```

3. **Run All Tests:**
   ```powershell
   dotnet test
   ```

4. **Run Tests with Detailed Output:**
   ```powershell
   dotnet test --verbosity normal
   ```

5. **Run Tests and See Results in Real-Time:**
   ```powershell
   dotnet test --logger "console;verbosity=detailed"
   ```

## Method 3: Visual Studio Code

1. Install the `.NET Core Test Explorer` extension
2. Open the test project folder
3. Tests will appear in the Test Explorer sidebar
4. Click the play button to run tests

## Troubleshooting

### If tests don't appear:
1. **Rebuild the solution:**
   - `Build` → `Rebuild Solution`

2. **Restore NuGet packages:**
   ```powershell
   dotnet restore
   ```

3. **Clean and rebuild:**
   ```powershell
   dotnet clean
   dotnet build
   dotnet test
   ```

### If you see "No test found":
1. Make sure the test project is included in the solution
2. Check that all NuGet packages are installed
3. Verify the project reference to the main project is correct

## Expected Output

When tests run successfully, you should see:
```
Passed!  - Failed:     0, Passed:    65, Skipped:     0, Total:    65
```

## Test Categories

- **Controller Tests:** Test all controller actions
- **Model Validation:** Test data validation rules
- **Business Logic:** Test status properties and calculations
- **Error Handling:** Test error scenarios
- **Integration:** Test end-to-end workflows

