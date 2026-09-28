import type {
    ReactNode
} from "react";

import AppHeader
    from "./AppHeader";

import campusBackground
    from "../Pages/Assets/campus-background-transparent.png";

interface PageShellProps {
    title: string;
    description?: string;
    children: ReactNode;
    showBack?: boolean;
}

const PageShell = (
    {
        title,
        description,
        children,
        showBack = true
    }: PageShellProps
) => (
    <main
        className="page-shell"
        style={{
            backgroundImage:
                `url(${campusBackground})`
        }}
    >

        <AppHeader
            title={title}
            showBack={showBack}
        />

        <div
            className="page-content"
        >
            <section
                className="page-heading"
            >
                <h1>
                    {title}
                </h1>

                {description && (
                    <p>
                        {description}
                    </p>
                )}
            </section>

            {children}

        </div>

    </main>
);

export default PageShell;