# Walkthrough: CR-0015 — PR #38 Properties Core Code Review

## Task Reference
- Task: `docs/tasks/cr-0015-pr38-properties-core-review.md`
- PR: #38 — feat(properties): Epic P2 — Properties core domain, API, media, SDK client

## Changes Made

### 1. License Headers (9 files)
Ran `scripts/add-license-headers.sh` to add missing headers to all frontend `.tsx`/`.ts` files in `apps/web/src/components/properties/json-forms/`.

### 2. EnergyRating Enum Alignment
Added `InProgress = 8` to `EnergyRating.cs` to match the frontend JSON schema and `EnergyRatingRenderer.tsx` component.

### 3. UploadMediaCommandHandler — Stream Seekability
Replaced direct `request.FileStream` usage with a `MemoryStream` copy pattern:
- Copy incoming stream to `MemoryStream` once
- Use `memoryStream.Position = 0` before each read operation
- Eliminates risk of `NotSupportedException` with non-seekable streams

### 4. Floor Plan Upload Limit
- Added `MaxFloorPlansPerProperty = 10` constant to `PropertyMedia.cs`
- Added `CountByPropertyIdAndMediaTypeAsync` to `IPropertyMediaRepository`
- Implemented in `PropertyMediaRepository`
- Added floor plan count check in `UploadMediaCommandHandler`

### 5. ReorderMedia Validation
- Added check: request must include ALL media items for the property
- Added check: display orders must be unique
- Both throw `DomainException` on violation

### 6. DTO Relocation
- Moved `ReorderRequest` and `ReorderItemRequest` from `PropertyMediaController.cs` to new file `Api/Dtos/PropertyMediaRequests.cs`
- Added `using Propely.PropertiesApi.Api.Dtos` to controller

## Validation

- `dotnet build services/properties-api/Propely.PropertiesApi.sln`
- `dotnet test services/properties-api/Propely.PropertiesApi.sln`
- `scripts/verify-license-headers.sh`
