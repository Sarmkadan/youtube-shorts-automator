# ApiCredentialRepositoryValidation

`ApiCredentialRepositoryValidation` provides a set of static helper methods for validating API credential data before it is persisted or used by the repository layer. The class centralises validation logic, ensuring consistent error handling and reducing duplication across the code‑base.

## API

### `public static IReadOnlyList<string> Validate`

Validates the current credential data and returns a read‑only list of validation error messages.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` containing one entry for each validation failure. If the list is empty, the data is considered valid.  
- **Exceptions:** This method does not throw; all validation results are reported via the returned collection.

### `public static bool IsValid`

Determines whether the credential data passes all validation rules.  
- **Parameters:** None.  
- **Return Value:** `true` if `Validate` returns an empty collection; otherwise `false`.  
- **Exceptions:** This method does not throw.

### `public static void EnsureValid`

Ensures that the credential data is valid, throwing an exception if any validation rule fails.  
- **Parameters:** None.  
- **Return Value:** None.  
- **Exceptions:** Throws `DomainException` (or a more specific subclass) when `Validate` returns one or more error messages. The exception message aggregates the validation errors.

## Usage

### Example 1 – Pre‑save validation

