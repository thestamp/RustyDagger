namespace RustyDagger.Shared.Static;

public static class ContentStrings
{
    public static readonly string[] Rumors = new[]
    {
        "They sez as Aileen Suitor is lookin' fer a husband.",
        "I heard Bill Smith used to be in the Dragon Guard.",
        "Silas Keeper is a decent sort, when sober.",
        "They sez Sally Trader gives ye a fair price.",
        "I heard that Fenton Magus eats small children.",
        "That Elden Bishop is a funny sort.",
        "Half of what these people tell ya are dead lies.",
        "The Queen is always lookin' for new Guardsmen.",
        "There's this little dwarf smithy out in the woods.",
        "I saw this strange cave in the woods.",
        "Be careful you don't end up in the Castle Dungeons.",
        "There is a dragon deep in the Abandoned Mines.",
        "Beware the wyverns, they steal your soul.",
        "Donatin' to the healer just makes ya poor.",
        "Donatin' to the healer is good fer the soul.",
        "Centaurs ain't a bad sort, just don't cross 'em.",
        "Watch them rats, them is filthy little buggers.",
        "Rats'll make yez sick.",
        "Be careful of mages, they have dyin' curses.",
        "Beware the curse of the wizard.",
        "Goblins ken be dengrus, if you bin drinkin. Hic.",
        "Never trust a gypsy.",
        "A little food can be handy for handlin' rats.",
        "It's hard getting' to the woods.",
        "If ye ever git to th' castle, stay outa the dunjin.",
        "Salves are great for really long fights.",
        "I hear that potions give you lots of energy.",
        "Like this knife? It's been enchanted.",
        "The queen is hundreds of years old...",
        "They call it the Arcane Forest.",
        "There's big ugly giants in the mountains.",
        "I've been to the mountains.  It's a long trip.",
        "Harpy wounds can fester up badly.",
        "Elves are uppity critters, but they will trade.",
        "There's another trader up in the mountains.",
        "I saw a troll under a bridge once.  Mean booger.",
        "I think maybe Fenton Magus knows a secret or two.",
        "They ain't no food in the mountains.  I bin there."
    };

    public static readonly (string Question, string Answer)[] Riddles = new[]
    {
        ("A drum that beats untouched, but halts when touched.", "A heart."),
        ("What walks on four legs in the morning, two legs in the afternoon and three legs in the evening?", "Man"),
        ("Why did the gryphon cross the road?", "To eat the puny mortal?"),
        ("Why can't centaurs dance?", "They have two left feet."),
        ("There's something in my pocket, but there is nothing in my pocket, whats in my pocket?", "There's a hole in your pocket."),
        ("It goes through an apple, It points out the way, It fits in a bow, Then a target, to stay.", "An arrow"),
        ("What can run but never walks, has a mouth but never talks, has a bed but never sleeps?", "A river"),
        ("What is it which belongs to you, but others use it more than you do?", "My name."),
        ("What is it that a dog does lifting a leg, a man does standing up, and a woman does sitting down?", "Shake hands."),
        ("What occurs once in a minute, twice in a moment and never in an hour?", "The letter 'M'"),
        ("Give it food and it will live, give it water and it will die.", "Fire"),
        ("The more you take, the more you leave behind.", "Footsteps"),
        ("I went into the woods and got it. I sat down to seek it. I brought it home because I could not find it.", "A splinter.")
    };

    public static readonly string[] WrongGuesses = new[]
    {
        "Uh... Reaganomics?",
        "Oatmeal!",
        "A power drill....",
        "Fine china.",
        "Lipo-suction?",
        "Green marbles, no - red ones!",
        "The Elephant Man?",
        "Was it Elvis?",
        "Fire ants!",
        "Thailand.",
        "Spitting?",
        "Fred's Friends, Inc. --right?",
        "Uhhh.... Uhhh.... Uh-oh...",
        "I used to know that one...",
        "Could you repeat that?",
        "I think I hear my mother calling.",
        "Beetlegoose! Beetlegoose!, Beetlegoose!",
        "A bottle nosed dolphin.",
        "An indian head penny.",
        "My sword in your gullet!"
    };

    public static readonly string[] SeduceOutcomes = new[]
    {
        "You share an intimite candlelight dinner for two.",
        "You take turns giving each other backrubs.",
        "You both hop into a hot, soapy bathtub!",
        "You exchange chaste kisses, giggling like teenagers.",
        "It writes you a love letter, in some foreign tongue.",
        "It humps your leg like a horny cocker spaniel.",
        "It belly dances for you, without asking for a tip.",
        "It sings you a romantic ballad in a high squeaky voice."
    };

    public static readonly string[] ControlActions = new[]
    {
        "leave it thinking it is a chicken.  Cluck, cluck.",
        "force its head to explode just like 'Scanners'",
        "send it on a road trip to Moscow for Vodka.",
        "tattoo \"I'm with stupid\" on its chest.",
        "take it to dinner and stick it with the bill.",
        "direct it to leap of the nearest cliff.",
        "order it to go chase Fenton Magus.",
        "send it out collecting flowers for its mother."
    };

    public static readonly string[] Races = { "Human", "Goblin", "Shide", "Elf", "Orc", "Dwarf", "Gypsy", "Gnome", "Troll", "Halfling" };
    public static readonly string[] Builds = { "Average", "Thin", "Heavy", "Muscular", "Slender", "Short", "Tall", "Stocky", "Athletic" };
    public static readonly string[] Signs = { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces" };
    public static readonly string[] Colors = { "Fair", "Tan", "Brown", "Black", "Red", "Golden", "Pale", "Olive", "Grey", "White", "Blue", "Green" };
    public static readonly string[] Habits = { "bites nails when nervous", "whistles off-key", "talks to self", "scratches head when confused", "hums constantly", "cracks knuckles", "fidgets" };
    public static readonly string[] Features = { "small scar above left eye", "missing left ear", "tattoo of a snake", "crooked nose", "dimpled chin", "bushy eyebrows", "freckles everywhere", "burn marks on hands", "an old battle scar", "a mysterious birthmark", "several missing teeth" };
    public static readonly string[] Phrases = { "Have at thee!", "Fortune favors the bold!", "Run while you can!", "Stand and deliver!", "By the Queen's grace!" };

    // Consumable item prices
    public static readonly (string Name, int Price)[] ShopItems = new[]
    {
        ("Food", 2),
        ("Fish", 4),
        ("Torch", 4),
        ("Rope", 6),
        ("Pen & Paper", 12),
        ("Sleeping Bag", 25),
        ("Cooking Gear", 50),
        ("Camp Tent", 150),
        ("Warrens Map", 1000),
        ("Treasury Map", 2500),
        ("Throne Room Map", 8000),
        ("Vortex Map", 10000),
        ("Rutter for Hie Brasil", 15000),
        ("Rutter for Shangala", 18000)
    };
}
