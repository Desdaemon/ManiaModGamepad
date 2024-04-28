using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace System
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random random)
        {
            return random.Next(2) == 1;
        }
    }

}

namespace osu.Game.Rulesets.Mania.Mods
{
    using HoldSpan = (double EndTime, int Column);
    internal class MapFixer(Random random)
    {
        internal HoldSpan?[] LeftHoldColumns { get; set; } = new HoldSpan?[2];
        internal HoldSpan?[] RightHoldColumns { get; set; } = new HoldSpan?[2];
        internal bool[] ActiveMask { get; set; } = new bool[6];
        private double currentTime = 0;
        private double delta = double.PositiveInfinity;
        // TODO: Calculate the threshold based on the standard deviation of map density. 
        internal bool DifficultPatterns => delta < 150;
        internal double CurrentTime
        {
            get => currentTime;
            set
            {
                delta = value - currentTime;
                currentTime = value;

                Array.Fill(ActiveMask, false);
                for (int i = 0; i < 2; i++)
                {
                    if (LeftHoldColumns[i] is HoldSpan hold)
                    {
                        if (currentTime <= hold.EndTime)
                            ActiveMask[hold.Column] = true;
                        if (hold.EndTime <= currentTime)
                            LeftHoldColumns[i] = null;
                    }
                    if (RightHoldColumns[i] is HoldSpan rhold)
                    {
                        if (rhold.EndTime >= currentTime)
                            ActiveMask[rhold.Column] = true;
                        if (rhold.EndTime <= currentTime)
                            RightHoldColumns[i] = null;
                    }
                }
            }
        }

        private bool[] lastRowMask = new bool[6];

        internal Random Rng { get; set; } = random;

        internal MapFixer(int seed = -1) : this(new Random(seed))
        {
        }

        internal IEnumerable<ManiaHitObject> ProcessRow(IGrouping<double, ManiaHitObject> row)
        {
            CurrentTime = row.Key;

            var newRow = new List<ManiaHitObject>(row.Count());
            var leftHand = row.Where(h => h.Column < 3).OrderBy(h => h.Column);
            var rightHand = row.Where(h => h.Column >= 3).OrderBy(h => h.Column);
            newRow.AddRange(ProcessHand(leftHand, leftHand: true));
            newRow.AddRange(ProcessHand(rightHand, leftHand: false));
            newRow.Sort((a, z) => a.Column.CompareTo(z.Column));

            bool[] newRowMask = new bool[6];
            Array.Copy(ActiveMask, newRowMask, 6);
            foreach (var ho in newRow)
                newRowMask[ho.Column] = true;

            if (DifficultPatterns && newRow.Any(h => lastRowMask[h.Column]))
            {
                newRow.RemoveAll(ho =>
                {
                    if (!lastRowMask[ho.Column] || ho is not Note) return false;

                    bool isColumnCompatible(int it)
                    {
                        return it != ho.Column
                            && !lastRowMask[it] && !newRowMask[it]
                            && (it >= 3
                                ? handIsCompatible(newRowMask[3..6], it)
                                : handIsCompatible(newRowMask[0..3], it));
                    }
                    int newColumn = ho.Column >= 3
                        ? Enumerable.Range(0, 6).FirstOrDefault(isColumnCompatible, -1)
                        : Enumerable.Range(0, 6).Reverse().FirstOrDefault(isColumnCompatible, -1);
                    if (newColumn != -1)
                    {
                        newRowMask[ho.Column] = false;
                        ho.Column = newColumn;
                        newRowMask[ho.Column] = true;
                    }
                    else
                    {
                        newRowMask[ho.Column] = false;
                        return true;
                    }
                    return false;
                });
            }

            newRow.TrimExcess();
            lastRowMask = newRowMask;
            return newRow;
        }

        internal IEnumerable<ManiaHitObject> ProcessHand(IEnumerable<ManiaHitObject> hand, bool leftHand)
        {
            selectHoldColumn(hand, leftHand, out var holdNote);
            var holdNotes = leftHand ? LeftHoldColumns : RightHoldColumns;
            int holdNotesCount = holdNotes.Count(ObjectExtensions.IsNotNull);
            if (holdNotesCount == 0 && holdNote == null)
            {
                foreach (var ho in ProcessHandNotes(hand.OfType<Note>(), leftHand))
                    yield return ho;
                yield break;
            }
            if (holdNote != null)
                yield return holdNote;

            if (holdNotesCount > 1)
                yield break;

            var holdSpan = holdNotes.First(ObjectExtensions.IsNotNull)!.Value;

            switch (holdSpan.Column % 3)
            {
                case 0 when !ActiveMask[holdSpan.Column + 1]:
                {
                    var singleNote = hand.FirstOrDefault(h => h.Column > holdSpan.Column);
                    if (singleNote.IsNotNull())
                        yield return maybeToNote(singleNote, leftHand: holdSpan.Column < 3, withColumn: holdSpan.Column + 1);
                    break;
                }
                case 1:
                {
                    // If holdNote is middle, take either the left or right single note, whichever exists and up to RNG
                    var singleNotes = hand.Where(n => n.Column != holdSpan.Column).ToArray();
                    if (singleNotes.Length > 0)
                        yield return maybeToNote(singleNotes[Rng.Next(singleNotes.Length)], leftHand: holdSpan.Column < 3);
                    break;
                }
                case 2 when !ActiveMask[holdSpan.Column - 1]:
                {
                    var singleNote = hand.FirstOrDefault(n => n.Column < holdSpan.Column);
                    if (singleNote.IsNotNull())
                        yield return maybeToNote(singleNote, leftHand: holdSpan.Column < 3, withColumn: holdSpan.Column - 1);
                    break;
                }
            }
        }

        internal IEnumerable<Note> ProcessHandNotes(IEnumerable<Note> hand, bool leftHand)
        {
            var orderedHand = hand.OrderBy(h => h.Column).ToArray();
            int[] columns = orderedHand.Select(h => h.Column).ToArray();
            if (columns.SequenceEqual(leftHand ? [0, 1, 2] : [3, 4, 5]))
            {
                if (Rng.NextBool())
                {
                    yield return orderedHand[0];
                    yield return orderedHand[1];
                }
                else
                {
                    yield return orderedHand[1];
                    yield return orderedHand[2];
                }
            }
            else if (columns.SequenceEqual(leftHand ? [0, 2] : [3, 5]))
            {
                int offset = leftHand ? 0 : 3;
                if (ActiveMask[leftHand ? 1 : 4])
                {
                    // edge case: sts (t = tail note)
                    // just pick either one without changing the column
                    yield return Rng.NextBool() ? orderedHand[0] : orderedHand[1];
                    yield break;
                }
                if (Rng.NextBool())
                {
                    yield return orderedHand[0];
                    if (!ActiveMask[1 + offset])
                        yield return withColumn(orderedHand[1], 1 + offset);
                }
                else
                {
                    if (!ActiveMask[1 + offset])
                        yield return withColumn(orderedHand[0], 1 + offset);
                    yield return orderedHand[1];
                }
            }
            else
            {
                bool[] activeMaskHand = leftHand ? ActiveMask[..3] : ActiveMask[3..];
                foreach (var ho in orderedHand)
                {
                    if (handIsCompatible(activeMaskHand, ho.Column))
                    {
                        activeMaskHand[ho.Column % 3] = true;
                        yield return ho;
                    }
                }
            }
        }

        private void selectHoldColumn(IEnumerable<ManiaHitObject> hand, bool leftHand, out HoldNote? holdNote)
        {
            holdNote = null;
            var holdColumns = leftHand ? LeftHoldColumns : RightHoldColumns;

            int nextFreeHoldSpace = Array.FindIndex(leftHand ? LeftHoldColumns : RightHoldColumns, ObjectExtensions.IsNull);
            if (nextFreeHoldSpace == -1) return;

            // TODO: Try multiple hold notes before returning
            holdNote = hand.OfType<HoldNote>().MaxBy(h => h.EndTime);
            if (holdNote.IsNull()) return;

            var siblingHold = holdColumns.FirstOrDefault(ObjectExtensions.IsNotNull);
            if (siblingHold is HoldSpan holdSpan)
            {
                switch (holdSpan.Column % 3)
                {
                    case 0:
                        holdNote.Column = holdSpan.Column + 1;
                        break;
                    case 2:
                        holdNote.Column = holdSpan.Column - 1;
                        break;
                }
            }
            if (ActiveMask[holdNote.Column])
            {
                holdNote = null;
                return;
            }

            holdColumns[nextFreeHoldSpace] = (holdNote.EndTime, holdNote.Column);
        }

        private static bool handIsCompatible(bool[] rows, int column)
        {
            if (rows[column % 3]) return false;
            switch (column % 3)
            {
                case 0:
                    return !rows[2];
                case 1:
                    return !rows[0] && !rows[2];
                case 2:
                    return !rows[0];
                default:
                    throw new UnreachableException();
            }
        }

        private static T withColumn<T>(T note, int column) where T : ManiaHitObject
        {
            note.Column = column;
            return note;
        }

        private static Note toNote(ManiaHitObject hitObject, int? withColumn = null)
        {
            if (hitObject is Note note)
            {
                note.Column = withColumn ?? note.Column;
                return note;
            }
            if (hitObject is HoldNote hold)
            {
                return new Note
                {
                    StartTime = hold.StartTime,
                    Column = withColumn ?? hold.Column,
                    Samples = hold.GetNodeSamples(0),
                    HitWindows = hold.HitWindows,
                };
            }
            throw new UnreachableException();
        }

        private ManiaHitObject maybeToNote(ManiaHitObject hitObject, bool leftHand, int? withColumn = null)
        {
            if (hitObject is HoldNote holdNote)
            {
                var holdColumns = leftHand ? LeftHoldColumns : RightHoldColumns;
                int nextFreeHoldSpace = Array.FindIndex(holdColumns, ObjectExtensions.IsNull);
                if (nextFreeHoldSpace == -1) return toNote(holdNote, withColumn);

                holdNote.Column = withColumn ?? holdNote.Column;
                if (leftHand)
                    holdColumns[nextFreeHoldSpace] = (holdNote.EndTime, holdNote.Column);
                else
                    holdColumns[nextFreeHoldSpace] = (holdNote.EndTime, holdNote.Column);
                return holdNote;
            }
            return toNote(hitObject, withColumn);
        }
    }
    public class ManiaModGamepad : Mod, IApplicableToBeatmap
    {
        public override string Name => "Gamepad";
        public override LocalisableString Description => @"Play with a gamepad!";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
        public override string Acronym => "GP";
        public override bool Ranked => false;

        public override Type[] IncompatibleMods =>
        [
            typeof(ManiaModRandom)
        ];

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var bm = (ManiaBeatmap)beatmap;
            if (bm.TotalColumns != 6) return;

            var rows = bm.HitObjects.OrderBy(h => h.StartTime).GroupBy(h => h.StartTime).ToImmutableArray();
            var newObjects = new List<ManiaHitObject>(bm.HitObjects.Count);

            var fixer = new MapFixer(bm.BeatmapInfo.ID.GetHashCode());
            foreach (var row in rows)
            {
                newObjects.AddRange(fixer.ProcessRow(row));
            }
            newObjects.TrimExcess();
            bm.HitObjects = newObjects;
        }
    }
}