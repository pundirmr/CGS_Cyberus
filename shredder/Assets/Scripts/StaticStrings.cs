public static class StaticStrings {
    public static readonly string[] IDs = new string[] { "P1", "P2", "P3" };
    public static readonly string[] Nums; // REVIEW(Zack): memory usage??
    public static readonly string[] ZeroNums = new string[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
    
    static StaticStrings() {
        int max = 1000; // max number is [999]
        Nums = new string[max];        
        for (int i = 0; i < max; ++i) {
            Nums[i] = i.ToString();
        }
    }
}
