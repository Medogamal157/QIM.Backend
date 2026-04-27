# Notifications and Background Jobs Plan

## Goal

Add a single notification system for all platform roles, with database persistence, optional real-time delivery, and email dispatch where the recipient may not be logged in.

## Recommended Stack

- Database tables: `Notifications`, `NotificationRecipients`, and optional `NotificationPreferences`.
- Real-time delivery: SignalR hub for admin/provider/client dashboards.
- Email delivery: queued outbox table plus retry worker.
- Background jobs: Hangfire for operational visibility and retries. Quartz.NET is also acceptable, but Hangfire is easier for dashboard monitoring.

## Role Events

### SuperAdmin

- New admin/user management actions requiring approval or audit.
- New business registrations, claims, flagged reviews, contact requests, and suggestions.
- Advertisement lifecycle events: scheduled, active, expired, paused.
- Background job failure alerts and repeated email delivery failures.

### Admin

- New business registration pending approval.
- New account verification request.
- New claim, flagged review, contact request, or suggestion.
- Business rejected/approved/suspended activity summaries.
- Daily digest of pending operational work.

### Moderator

- New pending or flagged reviews.
- New blog/content items requiring review, if editorial approval is added.
- Business content updates that need moderation.

### Support

- New contact request.
- Contact request assigned, moved to in-progress, resolved, or closed.
- New suggestion submitted and suggestion status changes.
- New business claim when customer follow-up may be required.

### Provider

- Business approved, rejected, suspended, or verified.
- New approved review on their business.
- Review flagged or removed after moderation.
- Claim accepted/rejected when it affects one of their businesses.
- Advertisement activation, expiration, or payment/action required.

### Client

- Account verified or verification rejected.
- Review approved, rejected, flagged, or removed.
- Claim submitted, approved, or rejected.
- Contact/suggestion response when email or account recipient is available.

## Background Jobs

- Email dispatch worker: sends queued email notifications, retries failed messages with backoff, and marks permanent failures.
- Advertisement lifecycle worker: activates scheduled ads and expires old ads automatically.
- Pending work reminders: reminds admins/support about stale pending businesses, claims, reviews, contact requests, suggestions, and account verifications.
- Admin digest worker: daily summary by role for pending approvals, flagged content, and support workload.
- Notification retention worker: archives or deletes old read notifications after the configured retention period.
- Job health monitor: alerts SuperAdmin when recurring jobs fail repeatedly.

## Implementation Phases

1. Add notification tables, DTOs, repository methods, and CRUD/read/unread endpoints.
2. Emit notifications from existing business approval, claim, review, contact, suggestion, account verification, and advertisement flows.
3. Add SignalR hub and frontend notification bell for admin, provider, and client layouts.
4. Add email outbox and Hangfire recurring jobs.
5. Add notification preferences and digest settings after the core system is stable.

## Review Decisions Needed

- Choose Hangfire or Quartz.NET.
- Choose email provider and sender domain.
- Decide retention period for read notifications.
- Decide whether admins receive instant notifications, daily digest, or both.
- Decide if clients/providers can opt out of non-critical notifications.