import type { Metadata } from "next";
import { AppQueryProvider } from "@/lib/react-query";
import "./globals.css";

export const metadata: Metadata = {
  title: "FlowMarket",
  description: "FlowMarket web app"
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AppQueryProvider>{children}</AppQueryProvider>
      </body>
    </html>
  );
}
