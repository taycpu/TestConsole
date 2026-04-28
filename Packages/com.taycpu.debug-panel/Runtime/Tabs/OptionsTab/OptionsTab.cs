using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DebugPanel.Options;

namespace DebugPanel.Tabs
{
    /// <summary>
    /// The Options Tab controller.
    /// Manages containers, scans options, and builds/rebuilds the scrollable list.
    /// </summary>
    public class OptionsTab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject categoryHeaderPrefab;
        [SerializeField] private GameObject optionRowPrefab;

        private readonly List<IOptionContainer> _containers = new List<IOptionContainer>();
        private readonly List<OptionDefinition> _standaloneOptions = new List<OptionDefinition>();
        private readonly List<GameObject> _spawnedRows = new List<GameObject>();

        private bool _dirty = true;

        private void OnEnable()
        {
            if (_dirty) Rebuild();
        }

        // ── Container / Option Management ─────────────────────────────────

        public void AddContainer(object container)
        {
            IOptionContainer optContainer;

            if (container is IOptionContainer oc)
            {
                optContainer = oc;
            }
            else
            {
                // Wrap plain C# object via reflection scanner
                optContainer = new ReflectedOptionContainer(container);
            }

            _containers.Add(optContainer);

            if (optContainer.IsDynamic)
            {
                optContainer.OptionAdded += _ => MarkDirty();
                optContainer.OptionRemoved += _ => MarkDirty();
            }

            // Subscribe to INotifyPropertyChanged for live refresh
            if (container is INotifyPropertyChanged npc)
                npc.PropertyChanged += OnContainerPropertyChanged;

            MarkDirty();
        }

        public void RemoveContainer(object container)
        {
            var toRemove = _containers.OfType<ReflectedOptionContainer>()
                .FirstOrDefault(c => c.Source == container)
                as IOptionContainer
                ?? _containers.FirstOrDefault(c => c == container as IOptionContainer);

            if (toRemove != null)
            {
                _containers.Remove(toRemove);
                if (container is INotifyPropertyChanged npc)
                    npc.PropertyChanged -= OnContainerPropertyChanged;
                MarkDirty();
            }
        }

        public void AddOption(OptionDefinition option)
        {
            _standaloneOptions.Add(option);
            MarkDirty();
        }

        public void RemoveOption(OptionDefinition option)
        {
            if (_standaloneOptions.Remove(option))
                MarkDirty();
        }

        private void MarkDirty()
        {
            _dirty = true;
            if (gameObject.activeInHierarchy)
                Rebuild();
        }

        private void OnContainerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Refresh just the value of the affected row rather than full rebuild
            RefreshAllValues();
        }

        // ── Build UI ──────────────────────────────────────────────────────

        private void Rebuild()
        {
            _dirty = false;

            // Clear existing rows
            foreach (var row in _spawnedRows)
                if (row != null) Destroy(row);
            _spawnedRows.Clear();

            if (contentRoot == null) return;

            // Gather all options
            var allOptions = new List<OptionDefinition>();
            foreach (var container in _containers)
                allOptions.AddRange(container.GetOptions());
            allOptions.AddRange(_standaloneOptions);

            // Sort: by category then sort order
            allOptions.Sort((a, b) =>
            {
                int catCmp = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
                return catCmp != 0 ? catCmp : a.SortOrder.CompareTo(b.SortOrder);
            });

            // Group by category and spawn rows
            string lastCategory = null;
            foreach (var opt in allOptions)
            {
                if (opt.Category != lastCategory)
                {
                    lastCategory = opt.Category;
                    SpawnCategoryHeader(opt.Category);
                }
                SpawnOptionRow(opt);
            }
        }

        private void SpawnCategoryHeader(string categoryName)
        {
            if (categoryHeaderPrefab == null) return;
            var go = Instantiate(categoryHeaderPrefab, contentRoot);
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = categoryName.ToUpper();
            _spawnedRows.Add(go);
        }

        private void SpawnOptionRow(OptionDefinition option)
        {
            if (optionRowPrefab == null) return;
            var go = Instantiate(optionRowPrefab, contentRoot);
            var renderer = go.GetComponent<OptionRowRenderer>();
            renderer?.Bind(option);
            _spawnedRows.Add(go);
        }

        private void RefreshAllValues()
        {
            foreach (var row in _spawnedRows)
            {
                if (row == null) continue;
                row.GetComponent<OptionRowRenderer>()?.RefreshValue();
            }
        }
    }

    /// <summary>Wraps a plain C# object for use as an IOptionContainer.</summary>
    internal class ReflectedOptionContainer : IOptionContainer
    {
        public object Source { get; }
        private readonly List<OptionDefinition> _options;

        public bool IsDynamic => false;
        public event Action<OptionDefinition> OptionAdded { add { } remove { } }
        public event Action<OptionDefinition> OptionRemoved { add { } remove { } }

        public ReflectedOptionContainer(object source)
        {
            Source = source;
            _options = OptionContainerScanner.Scan(source);
        }

        public IEnumerable<OptionDefinition> GetOptions() => _options;
    }
}
