CREATE PROCEDURE usp_SearchEmployees
    @DepartmentID INT = NULL,
    @IsActive BIT = NULL,
    @Search NVARCHAR(100) = NULL,
    @Page INT = 1,
    @PageSize INT = 20,
    @OutputFormat VARCHAR(10) = 'JSON'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- CTE to filter and join employee data
        WITH EmployeeCTE AS (
            SELECT 
                e.EmployeeID,
                e.FullName,
                d.DepartmentName,
                e.HireDate,
                e.IsActive
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentID = d.DepartmentID
            WHERE (@DepartmentID IS NULL OR e.DepartmentID = @DepartmentID)
              AND (@IsActive IS NULL OR e.IsActive = @IsActive)
              AND (@Search IS NULL OR e.FullName LIKE '%' + @Search + '%')
        )

        -- Paginated query output
        SELECT * 
        FROM EmployeeCTE
        ORDER BY HireDate DESC
        OFFSET (@Page - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY
        -- Output as JSON or XML depending on parameter
        -- JSON is default
        IF UPPER(@OutputFormat) = 'XML'
            FOR XML PATH('Employee'), ROOT('Employees'), ELEMENTS XSINIL;
        ELSE
            FOR JSON PATH, ROOT('Employees');

        COMMIT;
    END TRY
    BEGIN CATCH
        -- Rollback if an error occurs
        IF @@TRANCOUNT > 0
            ROLLBACK;

        -- Return error details
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
