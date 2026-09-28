interface ActionCardProps {
    title: string;

    description: string;

    onClick: () => void;

    emphasis?:
        | "default"
        | "warning";
}

const ActionCard = (
    {
        title,
        description,
        onClick,
        emphasis = "default"
    }: ActionCardProps
) => (
    <button
        className={
            `action-card ${
                emphasis === "warning"
                    ? "action-card-warning"
                    : ""
            }`
        }
        type="button"
        onClick={onClick}
    >
        <strong>
            {title}
        </strong>

        <span>
            {description}
        </span>

        <span
            className="action-arrow"
            aria-hidden="true"
        >
            →
        </span>
    </button>
);

export default ActionCard;