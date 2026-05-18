import { ErrorState } from "./ErrorState";
import { getTranslation } from "@/i18n";

export const RateLimitScreen = () => (
    <ErrorState
        icon="⏳"
        title={getTranslation("error.rate_limit.title")}
        description={getTranslation("error.rate_limit.description")}
        primaryAction={{
            label: getTranslation("error.rate_limit.retry"),
            onClick: () => window.location.reload(),
        }}
    />
);
