namespace QIM.Domain.Common.Enums;

public enum BusinessStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Suspended = 3
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3
}

public enum ClaimStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum ContactStatus
{
    New = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}

public enum SuggestionStatus
{
    New = 0,
    Reviewed = 1,
    Implemented = 2,
    Dismissed = 3
}

public enum BlogPostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum UserType
{
    Client = 0,
    Provider = 1,
    Admin = 2
}

public enum SearchIn
{
    All = 0,
    CompanyName = 1,
    ActivityCode = 2,
    Keywords = 3,
    ActivityName = 4
}

public enum SortBy
{
    HighestRated = 0,
    MostReviews = 1,
    Newest = 2
}
