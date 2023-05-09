public class Violation
{
    public enum Type {
        Stop,
        GiveWay,
        SpeedLimit,
        MissedRoadsign
    }

    public Type type;
    public double severity;
    public string description;

    public string ToJson() {
        return "{\"type\": \"" + type + "\", \"severity\": " + severity + ", \"description\": \"" + description + "\"}";
    }

    /* Returns wether two Violations are equal */
    public bool Equals(Violation input)
    {
        return this.type == input.type && this.severity == input.severity && this.description == input.description;
    }
}