import { useEffect, useId, useRef, useState } from "react";
import type { KeyboardEvent } from "react";
import "./DropdownExam.css";

export interface DropdownOption {
    value: string;
    label: string;
}

interface DropdownProps {
    options: DropdownOption[];
    value: string;
    onChange: (value: string) => void;
    ariaLabel?: string;
    ariaLabelledBy?: string;
    className?: string;
}

const DropdownExam = ({ options, value, onChange, ariaLabel, ariaLabelledBy, className = "" }: DropdownProps) => {
    const [open, setOpen] = useState(false);
    const [activeIndex, setActiveIndex] = useState(0);

    const rootRef = useRef<HTMLDivElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const listId = useId();

    const selectedIndex = options.findIndex(option => option.value === value);
    const selected = selectedIndex >= 0 ? options[selectedIndex] : undefined;

    // Close when clicking anywhere outside the dropdown.
    useEffect(() => {
        if (!open) return;

        const handlePointerDown = (event: MouseEvent) => {
            if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
                setOpen(false);
            }
        };

        document.addEventListener("mousedown", handlePointerDown);
        return () => document.removeEventListener("mousedown", handlePointerDown);
    }, [open]);

    // Keep the highlighted option visible when navigating with the keyboard.
    useEffect(() => {
        if (!open) return;
        const item = listRef.current?.children[activeIndex] as HTMLElement | undefined;
        item?.scrollIntoView({ block: "nearest" });
    }, [open, activeIndex]);

    const openList = () => {
        setActiveIndex(Math.max(selectedIndex, 0));
        setOpen(true);
    };

    const choose = (index: number) => {
        onChange(options[index].value);
        setOpen(false);
    };

    const handleKeyDown = (event: KeyboardEvent<HTMLButtonElement>) => {
        switch (event.key) {
            case "ArrowDown":
                event.preventDefault();
                if (!open) openList();
                else setActiveIndex(i => Math.min(i + 1, options.length - 1));
                break;
            case "ArrowUp":
                event.preventDefault();
                if (!open) openList();
                else setActiveIndex(i => Math.max(i - 1, 0));
                break;
            case "Home":
                if (open) {
                    event.preventDefault();
                    setActiveIndex(0);
                }
                break;
            case "End":
                if (open) {
                    event.preventDefault();
                    setActiveIndex(options.length - 1);
                }
                break;
            case "Enter":
            case " ":
                event.preventDefault();
                if (!open) openList();
                else choose(activeIndex);
                break;
            case "Escape":
                if (open) {
                    event.preventDefault();
                    setOpen(false);
                }
                break;
            case "Tab":
                setOpen(false);
                break;
        }
    };

    return (
        <div className={`dropdown ${className}`.trim()} ref={rootRef}>
            <button
                type="button"
                className="dropdown-button"
                role="combobox"
                aria-haspopup="listbox"
                aria-expanded={open}
                aria-controls={listId}
                aria-label={ariaLabel}
                aria-labelledby={ariaLabelledBy}
                aria-activedescendant={open ? `${listId}-${activeIndex}` : undefined}
                onClick={() => (open ? setOpen(false) : openList())}
                onKeyDown={handleKeyDown}
            >
                {selected ? selected.label : "\u00A0"}
            </button>

            {open && (
                <ul className="dropdown-list" role="listbox" id={listId} ref={listRef}>
                    {options.map((option, index) => (
                        <li
                            key={option.value}
                            id={`${listId}-${index}`}
                            role="option"
                            aria-selected={index === selectedIndex}
                            className={`dropdown-option${index === activeIndex ? " dropdown-option-active" : ""}`}
                            onMouseEnter={() => setActiveIndex(index)}
                            onMouseDown={event => event.preventDefault()}
                            onClick={() => choose(index)}
                        >
                            {option.label}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default DropdownExam;
