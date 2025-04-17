# Lunariens-Mental-Math-Trainer
Mental math trainer for Windows made in C#. You can train your mental calculation speed with it.

# Usage

<details>

<summary>Prerequisites for using to program</summary>

## Prerequisites
Before running the program, you will have to download a voice package for US English. To do this, follow these steps:
1. press Win+i to get into windows settings
2. In the search bar located somewhere at the top (for Windows 10) or top left (Windows 11), type "TTS" and select "Change text-to-speech settings"
![TTS searching](/README%20images/LMMT%20voice%20install%20guide.png) 
3. Find the section "Manage voices", and click "Add voices".
![Adding voices](/README%20images/LMMT%20add%20voices.png)
4. Search for "English" in the pop-up and select English (US) from the list.
![Search US English](/README%20images/search%20for%20english.png)

</details>
## In the program
When you start the program, you will be met with a menu screen with several action options. You can currently do the following:
* Train with a never-ending problem set
* View a statistic file (as a graph) from the *stats* folder by selecting it from a list
* View a statistic file (as a graph) from the *stats* folder by typing out the digit code of the problem stats saved in the file. Note that the name of the file corresponds to the problem statistics saved in it.
* View a statistic file (through a console menu) from the *stats* folder by selecting it from a list
* View a statistic file (through a console menu) folder by typing out the digit code of the problem stats saved in the file.
* Exit LMMT

When training in speech mode, you can press enter without having any text typed out to repeat the problem, in case you misheard or forgot it.





# Special Capabilities
* Text to speech capabilities (uncommon amongst other trainers)
* No silence at the end of the text-to-speech audio, making solve times much closer to correct. (unique to LMMT)
* Timing
* Statistics
* Graphing statistics

# Planned features
* Custom averages (like average of 10 problems, of 5, and arithmetic means)
* Bursts of problems (e. g. 10 problems at a time)
* Using multiple problem types in one training session
* More precise problem definitions (i. e. you'd tell the program something along the lines of "give me problems of this type: {number 20 to 35} to the power of {2 to 3}")
* Bigger number capabilities
* Bug fixes

# Limitations
* The program currently only accepts single-digit numbers for X, Y and Z in the digit code. This basically means that the problems can't have numbers with more than 9 digits.
* When entering a wrong answer, you don't get to try again. I might try to implement trying again sometime.