import { CheckIcon } from "./Icons.tsx";

interface StepIndicatorProps {
    steps: string[];
    current: number;
}

const StepIndicator = ({ steps, current }: StepIndicatorProps) => (
    <ol className="via-steps" aria-label="Import steps">
        {steps.map((label, index) => {
            const number = index + 1;
            const state = number < current ? "done" : number === current ? "active" : "todo";
            return (
                <li key={label} className={`via-step ${state}`} aria-current={state === "active" ? "step" : undefined}>
                    <span className="via-step-dot">{state === "done" && <CheckIcon />}</span>
                    <span className="via-step-label">{number}. {label}</span>
                </li>
            );
        })}
    </ol>
);

export default StepIndicator;
