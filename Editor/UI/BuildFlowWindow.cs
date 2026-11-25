using System;
using System.Collections.Generic;
using System.Linq;
using FurkanGok.BuildFlow.Build;
using FurkanGok.BuildFlow.Distribution;
using FurkanGok.BuildFlow.History;
using FurkanGok.BuildFlow.Profiles;
using FurkanGok.BuildFlow.Utilities;
using FurkanGok.BuildFlow.Validation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FurkanGok.BuildFlow.UI
{
    public sealed class BuildFlowWindow : EditorWindow
    {
        private enum Page
        {
            Dashboard,
            Profiles,
            Build,
            History
        }

        private BuildExecutionService _executionService;
        private BuildHistoryService _historyService;
        private BuildFlowProfile _selectedProfile;
        private ValidationReport _lastValidation;
        private BuildFlowBuildResult _lastResult;
        private Page _currentPage = Page.Dashboard;

        private VisualElement _content;
        private readonly Dictionary<Page, Button> _navButtons = new Dictionary<Page, Button>();

        [MenuItem(BuildFlowConstants.MenuRoot + "/Build Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildFlowWindow>();
            window.titleContent = new GUIContent(BuildFlowConstants.ProductName);
            window.minSize = new Vector2(880f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            _executionService = new BuildExecutionService();
            _historyService = new BuildHistoryService();
            _executionService.OperationChanged += _ => Repaint();
            _executionService.BuildCompleted += result =>
            {
                _lastResult = result;
                EditorApplication.delayCall += RefreshCurrentPage;
            };
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.AddToClassList("buildflow-root");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                $"Packages/{BuildFlowConstants.PackageId}/Editor/UI/BuildFlowWindow.uss");
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            BuildSidebar();
            _content = new VisualElement();
            _content.AddToClassList("buildflow-content");
            rootVisualElement.Add(_content);

            if (_selectedProfile == null)
            {
                var profiles = BuildFlowProfileFactory.FindAllProfiles();
                if (profiles.Length > 0)
                    _selectedProfile = profiles[0];
            }

            ShowPage(_currentPage);
        }

        private void BuildSidebar()
        {
            var sidebar = new VisualElement();
            sidebar.AddToClassList("buildflow-sidebar");
            rootVisualElement.Add(sidebar);

            var brand = new Label(BuildFlowConstants.ProductName);
            brand.AddToClassList("buildflow-brand");
            sidebar.Add(brand);

            var brandSub = new Label($"v{BuildFlowConstants.PackageVersion} · Editor");
            brandSub.AddToClassList("buildflow-brand-sub");
            sidebar.Add(brandSub);

            AddNav(sidebar, Page.Dashboard, "Dashboard");
            AddNav(sidebar, Page.Profiles, "Profiles");
            AddNav(sidebar, Page.Build, "Build");
            AddNav(sidebar, Page.History, "History");
        }

        private void AddNav(VisualElement sidebar, Page page, string label)
        {
            var button = new Button(() => ShowPage(page)) { text = label };
            button.AddToClassList("buildflow-nav-button");
            sidebar.Add(button);
            _navButtons[page] = button;
        }

        private void ShowPage(Page page)
        {
            _currentPage = page;
            foreach (var pair in _navButtons)
            {
                if (pair.Key == page)
                    pair.Value.AddToClassList("buildflow-nav-button--active");
                else
                    pair.Value.RemoveFromClassList("buildflow-nav-button--active");
            }

            _content.Clear();
            var scroll = new ScrollView();
            scroll.AddToClassList("buildflow-scroll");
            _content.Add(scroll);

            switch (page)
            {
                case Page.Dashboard:
                    BuildDashboard(scroll);
                    break;
                case Page.Profiles:
                    BuildProfilesPage(scroll);
                    break;
                case Page.Build:
                    BuildBuildPage(scroll);
                    break;
                case Page.History:
                    BuildHistoryPage(scroll);
                    break;
            }
        }

        private void RefreshCurrentPage()
        {
            if (rootVisualElement == null || _content == null)
                return;
            ShowPage(_currentPage);
        }

        private void BuildDashboard(VisualElement root)
        {
            AddPageHeader(root, "Dashboard", "Build activity and platform readiness for this project.");

            var history = _historyService.LoadHistory();
            var succeeded = history.Count(h => h.Status == BuildFlowResultStatus.Succeeded);
            var failed = history.Count(h =>
                h.Status == BuildFlowResultStatus.Failed ||
                h.Status == BuildFlowResultStatus.ValidationBlocked);
            var last = history.FirstOrDefault();

            var stats = Section(root, "Overview");
            var row = new VisualElement();
            row.AddToClassList("buildflow-stats-row");
            stats.Add(row);
            row.Add(Stat(history.Count.ToString(), "Total builds"));
            row.Add(Stat(succeeded.ToString(), "Successful"));
            row.Add(Stat(failed.ToString(), "Failed"));
            row.Add(Stat(last != null ? last.ProfileName : "—", "Last profile"));

            var lastSection = Section(root, "Last Build");
            if (last == null)
            {
                lastSection.Add(EmptyState("No builds yet. Create a profile and run validation from the Build page."));
            }
            else
            {
                lastSection.Add(InfoRow("Result", last.Result));
                lastSection.Add(InfoRow("Target", last.Target));
                lastSection.Add(InfoRow("Version", $"{last.ApplicationVersion} ({last.BuildNumber})"));
                lastSection.Add(InfoRow("Duration", $"{last.DurationSeconds:0.##}s"));
                lastSection.Add(InfoRow("Output", string.IsNullOrEmpty(last.OutputPath) ? "—" : last.OutputPath));
            }

            var platforms = Section(root, "Available Target Platforms");
            foreach (BuildFlowTargetPlatform platform in Enum.GetValues(typeof(BuildFlowTargetPlatform)))
            {
                var supported = PlatformSupportUtility.IsBuildTargetSupported(platform);
                platforms.Add(InfoRow(
                    PlatformSupportUtility.GetDisplayName(platform),
                    supported ? "Installed" : "Not installed"));
            }

            var recent = Section(root, "Recent Activity");
            if (history.Count == 0)
            {
                recent.Add(EmptyState("Recent build activity will appear here."));
            }
            else
            {
                foreach (var entry in history.Take(5))
                    recent.Add(InfoRow($"{entry.ProfileName} · {entry.Target}", $"{entry.Result} · {entry.StartTimeUtc}"));
            }

            var steam = Section(root, "Steam Distribution (Roadmap)");
            steam.Add(new Label(SteamDistributionRoadmap.Summary));
            var help = new Label("Planned: " + string.Join(", ", SteamDistributionRoadmap.PlannedCapabilities));
            help.AddToClassList("buildflow-help");
            steam.Add(help);
        }

        private void BuildProfilesPage(VisualElement root)
        {
            AddPageHeader(root, "Build Profiles", "Create and manage BuildFlow profiles stored in your project assets.");

            var toolbar = new VisualElement();
            toolbar.AddToClassList("buildflow-toolbar");
            root.Add(toolbar);

            toolbar.Add(new Button(() =>
            {
                _selectedProfile = BuildFlowProfileFactory.CreateProfile();
                ShowPage(Page.Profiles);
            }) { text = "Create Profile" });

            toolbar.Add(new Button(() =>
            {
                if (_selectedProfile == null)
                {
                    EditorUtility.DisplayDialog("BuildFlow", "Select a profile to duplicate.", "OK");
                    return;
                }

                _selectedProfile = BuildFlowProfileFactory.DuplicateProfile(_selectedProfile);
                ShowPage(Page.Profiles);
            }) { text = "Duplicate" });

            toolbar.Add(new Button(() =>
            {
                if (_selectedProfile == null)
                    return;
                Selection.activeObject = _selectedProfile;
                EditorGUIUtility.PingObject(_selectedProfile);
            }) { text = "Edit in Inspector" });

            toolbar.Add(new Button(() =>
            {
                if (_selectedProfile == null)
                    return;
                if (BuildFlowProfileFactory.DeleteProfile(_selectedProfile))
                {
                    _selectedProfile = BuildFlowProfileFactory.FindAllProfiles().FirstOrDefault();
                    ShowPage(Page.Profiles);
                }
            }) { text = "Delete" });

            var listSection = Section(root, "Profiles in Project");
            var profiles = BuildFlowProfileFactory.FindAllProfiles();
            if (profiles.Length == 0)
            {
                listSection.Add(EmptyState("No BuildFlow profiles found. Create one to get started. Profiles are saved under Assets/BuildFlow/Profiles/."));
                return;
            }

            var list = new VisualElement();
            list.AddToClassList("buildflow-list");
            listSection.Add(list);

            foreach (var profile in profiles.OrderBy(p => p.ProfileName))
            {
                var captured = profile;
                var item = new VisualElement();
                item.AddToClassList("buildflow-list-item");
                if (_selectedProfile == captured)
                    item.AddToClassList("buildflow-list-item--selected");

                var main = new VisualElement();
                main.AddToClassList("buildflow-list-item-main");
                var title = new Label(captured.ProfileName);
                title.AddToClassList("buildflow-list-item-title");
                var meta = new Label(
                    $"{PlatformSupportUtility.GetDisplayName(captured.TargetPlatform)} · {captured.BuildConfiguration} · v{captured.ApplicationVersion} ({captured.BuildNumber})");
                meta.AddToClassList("buildflow-list-item-meta");
                main.Add(title);
                main.Add(meta);
                item.Add(main);

                item.Add(new Button(() =>
                {
                    _selectedProfile = captured;
                    ShowPage(Page.Profiles);
                }) { text = "Select" });

                list.Add(item);
            }

            if (_selectedProfile != null)
            {
                var detail = Section(root, "Selected Profile");
                detail.Add(new InspectorElement(_selectedProfile));
            }
        }

        private void BuildBuildPage(VisualElement root)
        {
            AddPageHeader(root, "Build", "Validate and execute the selected BuildFlow profile.");

            var profiles = BuildFlowProfileFactory.FindAllProfiles();
            var config = Section(root, "Build Configuration");

            if (profiles.Length == 0)
            {
                config.Add(EmptyState("Create a BuildFlow profile before building."));
                config.Add(new Button(() =>
                {
                    _selectedProfile = BuildFlowProfileFactory.CreateProfile();
                    ShowPage(Page.Build);
                }) { text = "Create Profile" });
                return;
            }

            if (_selectedProfile == null)
                _selectedProfile = profiles[0];

            var profileField = new ObjectField("Profile")
            {
                objectType = typeof(BuildFlowProfile),
                value = _selectedProfile,
                allowSceneObjects = false
            };
            profileField.RegisterValueChangedCallback(evt =>
            {
                _selectedProfile = evt.newValue as BuildFlowProfile;
                _lastValidation = null;
                ShowPage(Page.Build);
            });
            config.Add(profileField);

            if (_selectedProfile != null)
            {
                config.Add(InfoRow("Target", PlatformSupportUtility.GetDisplayName(_selectedProfile.TargetPlatform)));
                config.Add(InfoRow("Configuration", _selectedProfile.BuildConfiguration.ToString()));
                config.Add(InfoRow("Version", _selectedProfile.ApplicationVersion));
                config.Add(InfoRow("Build number", _selectedProfile.BuildNumber));
                config.Add(InfoRow("Output", _selectedProfile.OutputDirectory));

                var scenes = BuildFlowSceneResolver.ResolveEnabledScenePaths(_selectedProfile);
                config.Add(InfoRow("Scenes", scenes.Length == 0 ? "None" : string.Join(", ", scenes)));
            }

            var validationSection = Section(root, "Validation");
            var validationToolbar = new VisualElement();
            validationToolbar.AddToClassList("buildflow-toolbar");
            validationSection.Add(validationToolbar);

            validationToolbar.Add(new Button(() =>
            {
                _lastValidation = _executionService.Validate(_selectedProfile);
                ShowPage(Page.Build);
            }) { text = "Validate" });

            if (_lastValidation == null)
            {
                validationSection.Add(EmptyState("Run validation to check this profile before building."));
            }
            else
            {
                validationSection.Add(InfoRow("Errors", _lastValidation.ErrorCount.ToString()));
                validationSection.Add(InfoRow("Warnings", _lastValidation.WarningCount.ToString()));
                foreach (var item in _lastValidation.Results)
                    validationSection.Add(ValidationItem(item));
            }

            var exec = Section(root, "Build Execution");
            var execToolbar = new VisualElement();
            execToolbar.AddToClassList("buildflow-toolbar");
            exec.Add(execToolbar);

            var buildButton = new Button(() => StartBuild()) { text = "Start Build" };
            buildButton.SetEnabled(!_executionService.IsBuilding);
            execToolbar.Add(buildButton);

            execToolbar.Add(new Button(() =>
            {
                if (_lastResult != null && !string.IsNullOrEmpty(_lastResult.OutputPath))
                    PathUtility.OpenInFileBrowser(_lastResult.OutputPath);
                else if (_selectedProfile != null)
                    PathUtility.OpenInFileBrowser(_selectedProfile.OutputDirectory);
            }) { text = "Open Output Folder" });

            exec.Add(InfoRow("Current operation", _executionService.CurrentOperation));

            if (_lastResult != null)
            {
                exec.Add(InfoRow("Last result", _lastResult.Result));
                exec.Add(InfoRow("Duration", $"{_lastResult.DurationSeconds:0.##}s"));
                exec.Add(InfoRow("Output size", _lastResult.OutputSizeDisplay ?? "—"));
                exec.Add(InfoRow("Warnings / Errors", $"{_lastResult.WarningCount} / {_lastResult.ErrorCount}"));
                exec.Add(InfoRow("Output path", _lastResult.OutputPath ?? "—"));
                if (!string.IsNullOrEmpty(_lastResult.ErrorSummary))
                    exec.Add(InfoRow("Summary", _lastResult.ErrorSummary));
                if (!string.IsNullOrEmpty(_lastResult.Notes))
                {
                    var notes = new Label(_lastResult.Notes);
                    notes.AddToClassList("buildflow-help");
                    exec.Add(notes);
                }
            }
            else
            {
                exec.Add(EmptyState("No build has been run in this session yet."));
            }
        }

        private void BuildHistoryPage(VisualElement root)
        {
            AddPageHeader(root, "Build History", "JSON reports stored in Library/BuildFlow/History/.");

            var toolbar = new VisualElement();
            toolbar.AddToClassList("buildflow-toolbar");
            root.Add(toolbar);
            toolbar.Add(new Button(RefreshCurrentPage) { text = "Refresh" });
            toolbar.Add(new Button(() => PathUtility.OpenInFileBrowser(_historyService.HistoryDirectory))
            {
                text = "Open History Folder"
            });

            var history = _historyService.LoadHistory();
            var section = Section(root, "Past Builds");
            if (history.Count == 0)
            {
                section.Add(EmptyState("No historical build reports yet."));
                return;
            }

            foreach (var entry in history)
            {
                var captured = entry;
                var item = new VisualElement();
                item.AddToClassList("buildflow-list-item");

                var main = new VisualElement();
                main.AddToClassList("buildflow-list-item-main");
                var title = new Label($"{captured.ProfileName} · {captured.Result}");
                title.AddToClassList("buildflow-list-item-title");
                var meta = new Label(
                    $"{captured.Target} · v{captured.ApplicationVersion} ({captured.BuildNumber}) · {captured.StartTimeUtc} · {captured.OutputSizeDisplay}");
                meta.AddToClassList("buildflow-list-item-meta");
                main.Add(title);
                main.Add(meta);
                item.Add(main);

                item.Add(new Button(() =>
                {
                    if (!string.IsNullOrEmpty(captured.ReportPath))
                        PathUtility.OpenInFileBrowser(captured.ReportPath);
                }) { text = "Open Report" });

                item.Add(new Button(() =>
                {
                    if (!string.IsNullOrEmpty(captured.OutputPath))
                        PathUtility.OpenInFileBrowser(captured.OutputPath);
                }) { text = "Output" });

                section.Add(item);
            }
        }

        private void StartBuild()
        {
            if (_selectedProfile == null)
            {
                EditorUtility.DisplayDialog("BuildFlow", "Select a profile first.", "OK");
                return;
            }

            _lastValidation = _executionService.Validate(_selectedProfile);
            if (_lastValidation.HasBlockingErrors)
            {
                EditorUtility.DisplayDialog(
                    "BuildFlow",
                    $"Build blocked by {_lastValidation.ErrorCount} validation error(s). Review the Validation section.",
                    "OK");
                ShowPage(Page.Build);
                return;
            }

            if (_executionService.IsBuilding)
                return;

            EditorUtility.DisplayProgressBar("BuildFlow", "Starting build…", 0.1f);
            try
            {
                _lastResult = _executionService.Execute(_selectedProfile);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            ShowPage(Page.Build);
        }

        private static void AddPageHeader(VisualElement root, string title, string subtitle)
        {
            var t = new Label(title);
            t.AddToClassList("buildflow-page-title");
            root.Add(t);
            var s = new Label(subtitle);
            s.AddToClassList("buildflow-page-subtitle");
            root.Add(s);
        }

        private static VisualElement Section(VisualElement root, string title)
        {
            var section = new VisualElement();
            section.AddToClassList("buildflow-section");
            var label = new Label(title);
            label.AddToClassList("buildflow-section-title");
            section.Add(label);
            root.Add(section);
            return section;
        }

        private static VisualElement Stat(string value, string label)
        {
            var box = new VisualElement();
            box.AddToClassList("buildflow-stat");
            var v = new Label(value);
            v.AddToClassList("buildflow-stat-value");
            var l = new Label(label);
            l.AddToClassList("buildflow-stat-label");
            box.Add(v);
            box.Add(l);
            return box;
        }

        private static VisualElement InfoRow(string label, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("buildflow-row");
            var l = new Label(label);
            l.AddToClassList("buildflow-row-label");
            var v = new Label(value ?? "—");
            v.AddToClassList("buildflow-row-value");
            row.Add(l);
            row.Add(v);
            return row;
        }

        private static VisualElement EmptyState(string message)
        {
            var label = new Label(message);
            label.AddToClassList("buildflow-empty");
            return label;
        }

        private static VisualElement ValidationItem(ValidationResult result)
        {
            var row = new VisualElement();
            row.AddToClassList("buildflow-validation-item");

            var badge = new Label(result.Passed ? "PASS" : result.Severity.ToString().ToUpperInvariant());
            badge.AddToClassList("buildflow-badge");
            if (result.Passed)
                badge.AddToClassList("buildflow-badge--pass");
            else if (result.Severity == ValidationSeverity.Error)
                badge.AddToClassList("buildflow-badge--error");
            else if (result.Severity == ValidationSeverity.Warning)
                badge.AddToClassList("buildflow-badge--warning");
            else
                badge.AddToClassList("buildflow-badge--info");
            row.Add(badge);

            var text = new VisualElement();
            text.AddToClassList("buildflow-validation-text");
            var title = new Label(result.RuleName);
            title.AddToClassList("buildflow-validation-title");
            var detail = new Label(result.DetailedMessage);
            detail.AddToClassList("buildflow-validation-detail");
            text.Add(title);
            text.Add(detail);
            row.Add(text);
            return row;
        }
    }
}
