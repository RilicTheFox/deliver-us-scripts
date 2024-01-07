public struct WorkdayStats
{
    public int DayNumber;
    
    public int Salary;

    public int UndeliveredParcels;
    public int UndeliveredParcelsPenalty;
    public bool AllParcelsDelivered;

    public int MisplacedParcels;
    public int MisplacedParcelsPenalty;
    
    public int DamagedParcels;
    public int DamagedParcelsPenalty;

    public int FinesIncurred;
    public int FinesIncurredPenalty;

    public int CorporatePropertyCharge;
    public int Bills;
    public int StudentDebt;

    public int TotalIncome()
    {
        int output = Salary - DamagedParcelsPenalty
                            - FinesIncurredPenalty
                            - CorporatePropertyCharge
                            - Bills
                            - StudentDebt;

        if (AllParcelsDelivered)
            output += UndeliveredParcelsPenalty;
        else
            output -= UndeliveredParcelsPenalty;

        return output;
    }
}