using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Diagnostics;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Detects circular references in the type pair graph using DFS cycle detection.
/// </summary>
internal static class CycleDetector
{
    /// <summary>
    /// Detects circular references in the type pair graph.
    /// Sets HasCyclicReference = true on all descriptors participating in a cycle.
    /// </summary>
    public static void DetectCycles(List<TypePairDescriptor> allTypePairs, SourceProductionContext context)
    {
        // Build a lookup: sourceFullName+destFullName -> descriptor
        var lookup = new Dictionary<string, TypePairDescriptor>(StringComparer.Ordinal);
        foreach (var tp in allTypePairs)
        {
            var key = tp.SourceFullName + "->" + tp.DestFullName;
            lookup[key] = tp;
        }

        // Build directed graph: edge from pair A to pair B if A has a Nested property that maps to B's types
        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var tp in allTypePairs)
        {
            var key = tp.SourceFullName + "->" + tp.DestFullName;
            var edges = new List<string>();

            foreach (var prop in tp.PropertyMatches)
            {
                if (prop.ConversionKind == ConversionKind.Nested)
                {
                    // The nested property maps source property type -> dest property type
                    var nestedKey = prop.SourcePropertyType + "->" + prop.DestPropertyType;
                    if (lookup.ContainsKey(nestedKey))
                    {
                        edges.Add(nestedKey);
                    }
                }
                else if (prop.ConversionKind == ConversionKind.Collection
                    && prop.SourceElementType is not null && prop.DestElementType is not null)
                {
                    var nestedKey = prop.SourceElementType + "->" + prop.DestElementType;
                    if (lookup.ContainsKey(nestedKey))
                    {
                        edges.Add(nestedKey);
                    }
                }
            }

            adjacency[key] = edges;
        }

        // DFS cycle detection (white/gray/black coloring)
        // 0 = white (unvisited), 1 = gray (in progress), 2 = black (done)
        var color = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var key in adjacency.Keys)
            color[key] = 0;

        var cycleNodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in adjacency.Keys)
        {
            if (color[key] == 0)
            {
                var path = new List<string>();
                DfsCycleDetect(key, adjacency, color, path, cycleNodes);
            }
        }

        // Mark all descriptors in cycles
        foreach (var key in cycleNodes)
        {
            if (lookup.TryGetValue(key, out var desc))
            {
                desc.HasCyclicReference = true;
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CircularReferenceDetected,
                    Location.None,
                    desc.SourceFullName,
                    desc.DestFullName,
                    desc.MaxDepth.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
        }
    }

    private static void DfsCycleDetect(
        string node,
        Dictionary<string, List<string>> adjacency,
        Dictionary<string, int> color,
        List<string> path,
        HashSet<string> cycleNodes)
    {
        color[node] = 1; // gray
        path.Add(node);

        if (adjacency.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (!color.ContainsKey(neighbor))
                    continue;

                if (color[neighbor] == 1) // back edge = cycle
                {
                    // Mark all nodes in the cycle (from neighbor to current)
                    var cycleStart = path.IndexOf(neighbor);
                    if (cycleStart >= 0)
                    {
                        for (int i = cycleStart; i < path.Count; i++)
                            cycleNodes.Add(path[i]);
                    }
                }
                else if (color[neighbor] == 0)
                {
                    DfsCycleDetect(neighbor, adjacency, color, path, cycleNodes);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        color[node] = 2; // black
    }
}
