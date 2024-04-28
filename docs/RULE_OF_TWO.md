# Rule of Two

###### What is this?

It's just a short name I've given for an observation from other 6K VSRGs like DJMax, Deemo Reborn and so on.
What makes it possible for such games to be played on controllers is very simple:

> 1. **No more than two notes** can be hit/held per hand.
> 2. **No brackets** i.e. `x_x`
> 3. If there's at least one note held in the hand, any additional note added must not contradict (1) and (2).

While there are violations of the rule even in official games, they are rarely done and only to
emphasize a certain moment in the track. Even with all of these limitations, however, it is possible to make maps both enjoyable and challenging. If you are a beatmap author, try applying these rules to your maps and see what kind of patterns you
can invent!

###### Why does this matter?

It is the mechanism by which this mod works: it either *moves* notes so as to maintain this rule throughout the
beatmap, or *removes* them if it is not possible to do so. If there are issues with converted maps violating
this rule, or causing Autoplay to not achieve 100% due to overlapping notes, we consider these to be bugs
and should be fixed by this mod.

###### How does this mod affect me?

Assuming you're not playing with your keyboard using this mod, here's some of my observations:

- There's a functional difficulty cap at around 5 stars where it starts becoming ridiculous or
  impossible to play anything. This may vary depending on your familiarity with playing VSRGs on controllers.
- Certain types of maps are significantly nerfed, especially jump-heavy and LN maps. Speed maps aren't
  affected as much since they don't tend to violate the Rule of Two, so they are as hard if not harder!
- Certain patterns don't violate the Rule of Two, but are still uncomfortable to play on a controller.
  This mod already includes some heuristics to reduce occurrences of hard/impossible patterns,
  but if something bothers you don't hesitate to open an issue!

Patterns that are known to be problematic:
- Bracket trills (in rapid succession)
```
x__
__x
x__
__x
```