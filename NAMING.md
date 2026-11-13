# Naming convention

**Name:** Ahmed Moharam Mohamed Kamal

## Test project, class, and file names

- Test projects end in `.UnitTests` or `.ArchitectureTests` and live under `tests/`.
- Test classes and files use the production type name followed by `Tests` (for example, `RentalMathTests`).
- Hand-written test doubles include their role in the name: `Dummy...`, `Stub...`, `Spy...`, `Mock...`, or `Fake...`.
- Bogus builders end in `Faker` (for example, `CustomerFaker`). AutoFixture customizations end in `Customization`; data attributes end in `DataAttribute`.

## Test method style

Compared styles:

1. `Method_Scenario_ExpectedResult`
2. `Should_ExpectedResult_When_Condition`
3. `Given_Condition_When_Action_Then_ExpectedResult`

I choose `Method_Scenario_ExpectedResult`: it names the method, the scenario, and the observable result in a compact form.

Examples from DriveNow:

- `CalculateAge_BirthdayHasNotOccurred_ReturnsOneYearLess`
- `CreateBooking_WhenVehicleIsAvailable_ReturnsBooking`
- `ProcessReturn_WhenDamageIsReported_IncludesDamageCharge`

## Production names and namespaces

- Interfaces start with `I` and live beside the abstraction's owning layer.
- Repository implementations end in `Repository` and live in a `Repositories` namespace.
- Application services end in `Service` and live in an application `Services` namespace.
- Domain exceptions end in `Exception` and live in `DriveNow.Domain.Exceptions` (or the matching ParkSmart domain namespace).
- Domain entities, value objects, and enums use singular PascalCase names and their respective `Entities`, `ValueObjects`, and `Enums` namespaces when those folders are used.
