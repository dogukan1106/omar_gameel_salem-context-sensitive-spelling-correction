Implementation of a sophisticated spell checker that makes use of language model to consider the context in which a word occurs.

**Context-sensitive spelling correction** is the task of fixing spelling errors that result in valid words, such as *I’d like to eat desert*, where **dessert** was typed when **desert** was intended.

These errors will go undetected by conventional spell checkers, which only flag words that are not found in a word list.

Context-sensitive spelling correction involves learning to characterize the linguistic contexts in which different words, such as **dessert** and **desert**, tend to occur. 

The problem is that there is a multitude of features one might use to characterize these contexts: features that test for the presence of a particular word nearby the target word; features that test the pattern of parts of speech around the target word; and so on. In general, the number of features will range from a few hundred to over ten thousands.

For more info check the [CodeProject article](http://www.codeproject.com/Articles/826449/Context-Sensitive-Spelling-Correction-using-Winnow)