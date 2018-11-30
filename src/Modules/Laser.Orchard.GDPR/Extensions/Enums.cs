namespace Laser.Orchard.GDPR.Extensions {
    public enum GDPRProcessOutcome {
        Unknown, Anonymized, Erased, Protected, Error
    }

    public enum EventForScheduling {
        Creation, LatestUpdate
    }

    public enum ScheduledProcess {
        Anonymization, Erasure
    }

    public enum CookieType {
        Technical, Preference, Statistical, Marketing
    }
}