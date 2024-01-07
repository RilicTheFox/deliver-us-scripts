using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameEndUI : MonoBehaviour
{
    public Action DayEndFinished;

    [Header("Day Count")]
    [SerializeField] private TMP_Text dayCount;
    [Header("Salary")]
    [SerializeField] private TMP_Text salaryAmount;
    [Header("Undelivered Parcels")]
    [SerializeField] private TMP_Text undeliveredParcelsNumber;
    [SerializeField] private TMP_Text undeliveredParcelsPenalty;
    [Header("Misplaced Parcels")]
    [SerializeField] private TMP_Text misplacedParcelsNumber;
    [SerializeField] private TMP_Text misplacedParcelsPenalty;
    [Header("Damaged Parcels")]
    [SerializeField] private TMP_Text damagedParcelsNumber;
    [SerializeField] private TMP_Text damagedParcelsPenalty;
    [Header("Fines Incurred")]
    [SerializeField] private TMP_Text finesIncurredNumber;
    [SerializeField] private TMP_Text finesIncurredPenalty;
    [Header("Bills")]
    [SerializeField] private TMP_Text corporatePropertyCharge;
    [SerializeField] private TMP_Text todaysBills;
    [SerializeField] private TMP_Text studentDebt;
    [Header("Total Income")]
    [SerializeField] private TMP_Text totalIncome;

    public void Populate(WorkdayStats stats)
    {
        dayCount.text = $"Day {stats.DayNumber} Complete!";
        
        salaryAmount.text = $"+ ¤{stats.Salary}";

        undeliveredParcelsNumber.text = $"Undelivered Parcels: {stats.UndeliveredParcels}";
        
        if (stats.AllParcelsDelivered)
            undeliveredParcelsPenalty.text = $"+ ¤{stats.UndeliveredParcelsPenalty}";
        else
            undeliveredParcelsPenalty.text = $"- ¤{stats.UndeliveredParcelsPenalty}";
        
        misplacedParcelsNumber.text = $"Misplaced Parcels: {stats.MisplacedParcels}";
        misplacedParcelsPenalty.text = $"- ¤{stats.MisplacedParcelsPenalty}";
        
        damagedParcelsNumber.text = $"Damaged Parcels: {stats.DamagedParcels}";
        damagedParcelsPenalty.text = $"- ¤{stats.DamagedParcelsPenalty}";

        finesIncurredNumber.text = $"Fines Incurred: {stats.FinesIncurred}";
        finesIncurredPenalty.text = $"- ¤{stats.FinesIncurredPenalty}";

        corporatePropertyCharge.text = $"- ¤{stats.CorporatePropertyCharge}";
        todaysBills.text = $"- ¤{stats.Bills}";
        studentDebt.text = $"- ¤{stats.StudentDebt}";

        totalIncome.text = $"¤{stats.TotalIncome()}";
    }

    public void OnNextDayClicked()
    {
        DayEndFinished?.Invoke();
    }
}
