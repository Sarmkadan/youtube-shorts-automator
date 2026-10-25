# VideoQuality

The `VideoQuality` enum names the output quality tier of a short in the console pipeline (`YouTubeShortAutomator` namespace, `src/Constants/VideoQuality.cs`). It is stored on `VideoShort.Quality` and is the value that `VideoShortExtensions.GetQualityDisplayName()` turns into a resolution label such as `1080p`.

The underlying type is `int`, and the members are assigned explicit values from `1` to `4`. Those values are persisted as integers by `VideoShortRepository`, so they must not be reordered or renumbered. The value `0` is not a member: `VideoShortRepositoryValidation` treats `default` as invalid, and `VideoShortValidation` rejects any value for which `Enum.IsDefined` is false.

## API

*   **`Low = 1`**
    The lowest tier, documented in the source as 360p at about 500 kbps. Displayed as `360p` by `GetQualityDisplayName()`.
*   **`Medium = 2`**
    The standard tier, documented as 720p at about 2000 kbps. Displayed as `720p`.
*   **`High = 3`**
    The tier documented as 1080p at about 5000 kbps. This is the value used in the console pipeline's sample job creation (`src/Program.cs`) and in the test fixtures. Displayed as `1080p`.
*   **`UltraHD = 4`**
    The highest tier, documented as 1440p at about 10000 kbps. Displayed as `1440p`.

The resolution and bitrate figures exist only in the source comments. The enum itself carries no data, and nothing in the console pipeline reads a bitrate from `VideoQuality`. Any encoder settings are chosen elsewhere.

## Usage

### Getting a display label for a short

`GetQualityDisplayName()` maps the four known members to their resolution labels. Any other value falls back to `Quality.ToString()`.

```csharp
var label = videoShort.GetQualityDisplayName(); // "1080p" when Quality is High
```

### Validating a quality value

`Enum.IsDefined` is the check used by the validation code. It rejects `0` and any number outside the four members.

```csharp
bool valid = Enum.IsDefined(typeof(VideoQuality), value.Quality);
```

## Notes

*   **Underlying type**: The enum does not declare a base type, so it uses `int`. Members have explicit values `1` to `4`. Do not renumber existing members; append any new tier with the next free value and update `GetQualityDisplayName()` to match.
*   **Stored value**: `VideoShortRepository` writes the enum as an `int` into the `Quality` column and reads it back with a cast, so the numeric values are part of the stored data.
*   **Default value**: `default(VideoQuality)` is `0`, which is not defined. A `VideoShort` created without setting `Quality` will fail validation.
