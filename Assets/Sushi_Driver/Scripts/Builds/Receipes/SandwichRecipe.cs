public struct SandwichRecipe
{
    public int FishA { get; }
    public int FishB { get; }
    public int FishC { get; }

    public SandwichRecipe(int fishA, int fishB, int fishC)
    {
        FishA = fishA;
        FishB = fishB;
        FishC = fishC;
    }

    public int CookIngredientOne()
    {
        int requiredFishA = 2;
        int resultA = FishA - requiredFishA;
        return resultA;
    }
    public int CookIngredientTwo()
    {
        int requiredFishB = 1;
        int resultB = FishB - requiredFishB;
        return resultB;
    }
    public int CookIngredientThree()
    {
        int requiredFishC = 0;
        int resultC = FishC - requiredFishC;
        return resultC;
    }
}
