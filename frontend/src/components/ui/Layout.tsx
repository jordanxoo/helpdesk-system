import type { ReactNode } from "react";
import Navbar from "./Navbar";

interface LayoutProps{
    children: ReactNode;
    currentPage?: string;
}

export default function Layout( {children,currentPage}: LayoutProps)
{
    return(
        <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
            <Navbar currentPage={currentPage}/>
            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {children}
            </main>
        </div>
    );
}