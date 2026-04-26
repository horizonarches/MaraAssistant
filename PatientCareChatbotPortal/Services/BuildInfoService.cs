using System.Text.Json;
using PatientCareChatbotPortal.Models;

namespace PatientCareChatbotPortal.Services;

public sealed class BuildInfoService
{
    private BuildInfo? _cached;

    public async Task<BuildInfo> GetBuildInfoAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("buildinfo.json");
            var info = await JsonSerializer.DeserializeAsync<BuildInfo>(stream, cancellationToken: cancellationToken)
                       ?? CreateFallbackBuildInfo();

            _cached = info;
            return info;
        }
        catch (FileNotFoundException)
        {
            _cached = CreateFallbackBuildInfo();
            return _cached;
        }
        catch (DirectoryNotFoundException)
        {
            _cached = CreateFallbackBuildInfo();
            return _cached;
        }
        catch (JsonException)
        {
            _cached = CreateFallbackBuildInfo();
            return _cached;
        }
    }

    private static BuildInfo CreateFallbackBuildInfo()
    {
        return new BuildInfo
        {
            BuildSummary = "Mara - A Patient Care Chatbot for the Room",
            AuthorCopyright = "Author: Lawrence Zalewski. All rights reserved.",
            AppDescription = "Mara is a chatbot assistant built to assist patients by making their hospital room a healing place, and providing a non-medical companion to aid in recovery."
        };
    }
}
