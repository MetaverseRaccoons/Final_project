using UnityEngine;

public class ViolationReporter {
    public static void ReportViolation(Violation violation) {
        Debug.Log(violation.ToJson());
    }
}