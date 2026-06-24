interface ActivityLayoutProps {
    children: React.ReactNode;
    header?: React.ReactNode;
}

export const ActivityLayout = ({ children, header }: ActivityLayoutProps) => {
    return (
        <div className="min-h-screen bg-background flex flex-col">
            {header}
            <main className="w-full px-4 sm:px-6 lg:px-8 py-8 flex flex-col flex-1">
                <div className="max-w-7xl w-full mx-auto">
                    {children}
                </div>
            </main>
        </div>
    );
};
