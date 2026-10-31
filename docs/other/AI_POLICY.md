# AI Policy

## A world I miss

There is a particular pleasure in building something yourself. Not the finished thing, necessarily. The part before that, the journey from the spark of the idea to when you merge your PR. Drawing the wrong version. Throwing it out. Finding the one that holds. And learning things the hard way.

I miss when that was simply what engineering was. I am not naive enough to think it was ever pure, or that everyone had the time to work that way. But the pleasure was the point. The understanding was the point.

We do not get to go back to that world. AI is here, it is in the editors, in the browsers, in the issue trackers, and the genie is not going back into the bottle. The world has to adapt. Lumina has to adapt too. This document is about how.

## It is a tool

A compiler is a tool. A debugger is a tool. A search engine, a linter, a calculator, a colleague who has seen the error before: tools. AI is one more thing on the bench. It is useful. It is not magic, and it is not a person.

Use it to get unstuck. Use it to sketch an approach, to remember an API, to translate a paragraph, to generate the boring first draft of a test list. There is no prize for suffering through everything alone.

But a tool does not own the result. It does not know why this codebase does things the boring way, it does not know which "pragmatic" or "efficient" shortcut will wake someone up at 3 AM, it does not have good taste and it does not have to maintain what it wrote. You do.

## The parts that worry me

The ethics of this are a mess... These models were trained on a lot of work that nobody handed over voluntarily, my own included. Call it what you want; I think _theft_ covers it. And I'm _annoyed_ that the internet is filling up with text that sounds certain and says nothing, I'm annoyed by stepping in slop everywhere, and I am angered by the _c'est la vie_ attitude towards the environmental issues that were already bad enough. I have not even mentioned the people whose jobs are being cut, or the fact that a handful of sociopathic companies now sit in the middle of all of it.

I do not know what to do about any of that. Banning the tool would not undo the training or put the water back, and pretending the problem is solved because I refuse to use it feels like a nice story I would be telling myself. Using it and shrugging is not better. So, I use it, I try not to be smug about it in either direction, and I do not lecture people who decide differently. It's for sure not a solution, it's where I have landed for now.

## The line must be drawn here! No further!!!

I only care about this:

**DON'T WASTE MY TIME!**

**The tool can help you think. It cannot do the thinking for you.**

If you cannot explain your change without opening a chat window, you do not understand it yet. If you cannot say why it works, what it breaks, and what happens when the input is wrong, then you are not the author. You are a delivery service, and I have no time, patience or desire to talk to your claude subscription through you. I promise, I am smart enough to do that myself, directly, without any intermediaries, if and when I chose to.

That is the whole policy, really. Everything below is just what it looks like in practice.

## What I expect from contributors

Lumina is a hobby project with professional standards. Contributions are welcome. They are not required, and they are not needed. Nobody owes this project a pull request. The project does not owe anybody a merge.

I know it is tempting to put a line on a resume or a green square on a chart, but before any of that, a pull request is a request for someone else's time - MY time. If you are here because you want to help build the thing, thank you! If you are here to produce output, this is the wrong project.

The standards here are high, draconian actually, and they are not up for negotiation. They are in [CONTRIBUTING.md](../CONTRIBUTING.md), along with the architecture, the style rules, and the test conventions. Read them. Follow them. A contribution that does not follow them gets rejected, and the fact that a model produced it in ten seconds does not make it cheaper for me to review. If anything, it makes it more expensive.

## If you use AI here

You can. There is no disclosure requirement. I do not need (or care) to know which model, which plugin, or how many prompts.

What I care about is the result. It should be indistinguishable from work you did yourself, not merely good for a machine or acceptable as a draft. It should leave no hint that a model was in the room, not because you are hiding it, but because you have digested the output and made it yours. If I can see the seams, the work is not finished, a tool's output is raw material, and raw material is not a contribution until someone with judgment turns it into one.

So:

- Read every line. If you cannot defend it in review, DO. NOT. send it!
- Build it and run the tests. Add tests that describe behavior. A test that only mirrors the implementation is decoration.
- Check the names, the errors, the edge cases, the documentation. The model did not think about them. It does not know they exist.
- Make it fit Lumina. The model does not know our conventions unless you feed them in, and it will drift anyway. Fitting it is your job. And, in case you haven't figured it out already, conventions, style and consistency are at the top of the list in this repository. Sorry, but not sorry: my repository, my rules.

## Keep your chatbot out of my inbox

As already stated, I am not interested in talking to your chatbot.

Do not open issues, pull requests, or comments from a bot account. Do not paste a model's answer into a review thread and call it your argument. Do not ask a model to reply to my comments for you. Do not point an agent at the repository and let it open pull requests or post comments on your behalf. If you want to contribute, show up. If you cannot show up, wait until you can.

This is not a manners thing. A conversation with a model is not a conversation with you. It hides who is accountable, it turns review into theater, and it wastes the one resource this project cannot manufacture: my attention. I would rather read three awkward sentences from a person than a polished page from a machine.

The same applies to bugs and security reports. If a model helped you find something, verify it before you send it. Do not send a guess with a reproduction you never ran. Do not send an invented issue with invented details. A false report is worse than no report, because it spends time that a real report needed.

## What gets rejected

I will reject a contribution, and I will not spend an hour explaining why, when:

- You cannot explain it, or the explanation is "the model said so".
- It ignores CONTRIBUTING.md, the architecture, the style, the tests or it invents new "standards" for no good reason.
- It is obviously generated and obviously unedited.
- It invents an API, a file, a behavior, evidence, or a source that does not exist.
- It reopens a rejected change with the same argument, especially an argument a model wrote for you.
- It is a low-effort patch that moves the work of understanding onto the reviewer.

I may close it without a long explanation. If it becomes a pattern, I may block the account. That is not a judgment on your character. It is a boundary around my time.

## What I am not saying

None of this is a ban. I am not trying to make anyone feel bad about using the tool either. Maintainers here can use whatever helps them, the same as anyone else, and they answer for the result. You do not have to start from a blank page. You do have to understand, check, and own what you send. That is the whole ask.

I am also not promising to review everything. Time is finite, and a bad patch can sit there unanswered.

## One more time, so it sticks:

I do not care which tool you used, I care whether the work is good, whether you understand it, whether you can stand behind it when someone asks a hard question, and whether it follows the standards and styles of the existing code.

The genie is out of the bottle, i get it. That does not mean the standards go out with it.
