import { useState, useEffect } from "react";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";
import { ScrollArea } from "@/shared/components/ui/scroll-area";
import {
  History,
  Trash2,
  Delete,
  Sparkles,
  Sun,
  Moon,
  LogOut,
} from "lucide-react";
import { useTheme } from "@/app/theme-provider";

interface HistoryItem {
  id: string;
  formula: string;
  result: string;
}

interface CalculatorProps {
  onLogout: () => void;
}

export function Calculator({ onLogout }: CalculatorProps) {
  const { theme, setTheme } = useTheme();
  const [display, setDisplay] = useState<string>("0");
  const [formula, setFormula] = useState<string>("");
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [showHistory, setShowHistory] = useState<boolean>(false);
  const [shouldReset, setShouldReset] = useState<boolean>(false);

  const isDark =
    theme === "dark" ||
    (theme === "system" &&
      window.matchMedia("(prefers-color-scheme: dark)").matches);

  // Load history from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem("calc_history");
    if (saved) {
      try {
        setHistory(JSON.parse(saved));
      } catch (e) {
        console.error("Failed to parse history", e);
      }
    }
  }, []);

  // Save history to localStorage
  const saveHistory = (newHistory: HistoryItem[]) => {
    setHistory(newHistory);
    localStorage.setItem("calc_history", JSON.stringify(newHistory));
  };

  // Safe Math Evaluator (Tokenizes and evaluates standard arithmetic with precedence)
  const evaluateExpression = (expr: string): number => {
    // Clean up expression
    let sanitized = expr
      .replace(/×/g, "*")
      .replace(/÷/g, "/")
      .replace(/\s+/g, "");

    // Simple Recursive Descent Parser
    let index = 0;

    const parseNumber = (): number => {
      let start = index;
      if (sanitized[index] === "-") index++; // Handle negative numbers
      while (
        index < sanitized.length &&
        ((sanitized[index] >= "0" && sanitized[index] <= "9") ||
          sanitized[index] === ".")
      ) {
        index++;
      }
      let val = parseFloat(sanitized.substring(start, index));
      if (isNaN(val)) return 0;
      return val;
    };

    const parseFactor = (): number => {
      if (sanitized[index] === "(") {
        index++; // Skip '('
        let val = parseExpression();
        index++; // Skip ')'
        return val;
      }
      return parseNumber();
    };

    const parseTerm = (): number => {
      let val = parseFactor();
      while (index < sanitized.length) {
        let op = sanitized[index];
        if (op === "*" || op === "/") {
          index++;
          let nextVal = parseFactor();
          if (op === "*") {
            val *= nextVal;
          } else {
            val = nextVal === 0 ? NaN : val / nextVal;
          }
        } else if (op === "%") {
          index++;
          // modulo or percentage depending on context. Let's make it standard modulo.
          let nextVal = parseFactor();
          val %= nextVal;
        } else {
          break;
        }
      }
      return val;
    };

    const parseExpression = (): number => {
      let val = parseTerm();
      while (index < sanitized.length) {
        let op = sanitized[index];
        if (op === "+" || op === "-") {
          index++;
          let nextVal = parseTerm();
          if (op === "+") {
            val += nextVal;
          } else {
            val -= nextVal;
          }
        } else {
          break;
        }
      }
      return val;
    };

    const result = parseExpression();
    return result;
  };

  const handleDigit = (digit: string) => {
    if (shouldReset) {
      setDisplay(digit);
      setShouldReset(false);
    } else {
      if (display === "0") {
        setDisplay(digit);
      } else {
        setDisplay(display + digit);
      }
    }
  };

  const handleDecimal = () => {
    if (shouldReset) {
      setDisplay("0.");
      setShouldReset(false);
      return;
    }
    if (!display.includes(".")) {
      setDisplay(display + ".");
    }
  };

  const handleOperator = (op: string) => {
    setShouldReset(true);

    // If we just got a calculation result, build on it
    if (formula.includes("=")) {
      setFormula(`${display} ${op} `);
      return;
    }

    if (display === "0" && formula === "") {
      if (op === "-") {
        setDisplay("-");
        setShouldReset(false);
      }
      return;
    }

    // Append to formula
    setFormula(`${formula}${display} ${op} `);
  };

  const handleClear = () => {
    setDisplay("0");
    setFormula("");
    setShouldReset(false);
  };

  const handleBackspace = () => {
    if (shouldReset) {
      setFormula("");
      return;
    }
    if (display.length > 1) {
      setDisplay(display.slice(0, -1));
    } else {
      setDisplay("0");
    }
  };

  const handleToggleSign = () => {
    if (display !== "0" && display !== "-") {
      if (display.startsWith("-")) {
        setDisplay(display.substring(1));
      } else {
        setDisplay("-" + display);
      }
    }
  };

  const handlePercentage = () => {
    const val = parseFloat(display);
    if (!isNaN(val)) {
      setDisplay((val / 100).toString());
      setShouldReset(true);
    }
  };

  const handleEqual = () => {
    if (formula === "" || formula.includes("=")) return;

    const fullExpression = `${formula}${display}`;
    const result = evaluateExpression(fullExpression);

    let resultString = "";
    if (isNaN(result)) {
      resultString = "Error";
    } else {
      // Limit decimals to 8 places to avoid display overflow, trim trailing zeroes
      resultString = Number(result.toFixed(8)).toString();
    }

    setDisplay(resultString);
    setFormula(`${fullExpression} = `);
    setShouldReset(true);

    if (resultString !== "Error") {
      const newItem: HistoryItem = {
        id: Date.now().toString(),
        formula: fullExpression,
        result: resultString,
      };
      saveHistory([newItem, ...history].slice(0, 50)); // Keep last 50 items
    }
  };

  const clearHistory = () => {
    saveHistory([]);
  };

  const reuseHistoryItem = (item: HistoryItem) => {
    setDisplay(item.result);
    setFormula("");
    setShouldReset(true);
  };

  // Keyboard support
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const key = e.key;

      if (key >= "0" && key <= "9") {
        handleDigit(key);
      } else if (key === ".") {
        handleDecimal();
      } else if (key === "+") {
        handleOperator("+");
      } else if (key === "-") {
        handleOperator("-");
      } else if (key === "*") {
        handleOperator("×");
      } else if (key === "/") {
        handleOperator("÷");
      } else if (key === "%") {
        handlePercentage();
      } else if (key === "Enter" || key === "=") {
        e.preventDefault();
        handleEqual();
      } else if (key === "Backspace") {
        handleBackspace();
      } else if (key === "Escape") {
        handleClear();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [display, formula, shouldReset]);

  return (
    <div className="flex justify-center items-center w-full min-h-[500px] p-4">
      <Card className="relative w-full max-w-md overflow-hidden bg-card/65 backdrop-blur-xl border border-border/80 shadow-2xl rounded-2xl transition-all duration-300">
        {/* Header with Title & History Toggle */}
        <div className="flex justify-between items-center px-6 pt-6 pb-2">
          <div className="flex items-center gap-2">
            <div className="p-1.5 rounded-lg bg-primary/10 text-primary">
              <Sparkles className="size-4" />
            </div>
            <span className="font-bold text-sm tracking-wider uppercase opacity-80">
              Neumorphic Calc
            </span>
          </div>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setTheme(isDark ? "light" : "dark")}
              className="rounded-full opacity-70 hover:opacity-100 transition-all duration-300 text-foreground"
              title="Toggle theme"
            >
              {isDark ? (
                <Sun className="size-4" />
              ) : (
                <Moon className="size-4" />
              )}
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setShowHistory(!showHistory)}
              className={`rounded-full transition-all duration-300 ${
                showHistory
                  ? "bg-primary/20 text-primary hover:bg-primary/30"
                  : "opacity-70 hover:opacity-100"
              }`}
              title="Calculation history"
            >
              <History className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={onLogout}
              className="rounded-full opacity-70 hover:opacity-100 text-destructive hover:bg-destructive/10 transition-all duration-300"
              title="Sign out"
            >
              <LogOut className="size-4" />
            </Button>
          </div>
        </div>

        <CardContent className="p-6 pt-2">
          {/* Display screen */}
          <div className="flex flex-col items-end justify-end min-h-[100px] px-4 py-3 mb-6 bg-slate-950/20 dark:bg-slate-950/40 border border-border/40 rounded-xl overflow-hidden text-right">
            <div className="text-xs text-muted-foreground font-medium min-h-[16px] tracking-wide break-all max-w-full">
              {formula}
            </div>
            <div className="text-4xl font-semibold tracking-tight text-foreground truncate max-w-full font-mono mt-1">
              {display}
            </div>
          </div>

          {/* Calculator Grid */}
          <div className="grid grid-cols-4 gap-2">
            {/* Row 1 */}
            <Button
              variant="secondary"
              onClick={handleClear}
              className="h-14 font-semibold text-base rounded-xl transition-all duration-200 active:scale-95 bg-secondary/80 hover:bg-secondary text-destructive dark:text-red-400"
            >
              AC
            </Button>
            <Button
              variant="secondary"
              onClick={handleToggleSign}
              className="h-14 font-semibold text-base rounded-xl transition-all duration-200 active:scale-95 bg-secondary/80 hover:bg-secondary text-foreground/80"
            >
              +/-
            </Button>
            <Button
              variant="secondary"
              onClick={handlePercentage}
              className="h-14 font-semibold text-base rounded-xl transition-all duration-200 active:scale-95 bg-secondary/80 hover:bg-secondary text-foreground/80"
            >
              %
            </Button>
            <Button
              variant="default"
              onClick={() => handleOperator("÷")}
              className="h-14 font-semibold text-xl rounded-xl transition-all duration-200 active:scale-95 bg-primary/90 text-primary-foreground hover:bg-primary"
            >
              ÷
            </Button>

            {/* Row 2 */}
            <Button
              variant="outline"
              onClick={() => handleDigit("7")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              7
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("8")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              8
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("9")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              9
            </Button>
            <Button
              variant="default"
              onClick={() => handleOperator("×")}
              className="h-14 font-semibold text-xl rounded-xl transition-all duration-200 active:scale-95 bg-primary/90 text-primary-foreground hover:bg-primary"
            >
              ×
            </Button>

            {/* Row 3 */}
            <Button
              variant="outline"
              onClick={() => handleDigit("4")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              4
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("5")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              5
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("6")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              6
            </Button>
            <Button
              variant="default"
              onClick={() => handleOperator("-")}
              className="h-14 font-semibold text-xl rounded-xl transition-all duration-200 active:scale-95 bg-primary/90 text-primary-foreground hover:bg-primary"
            >
              -
            </Button>

            {/* Row 4 */}
            <Button
              variant="outline"
              onClick={() => handleDigit("1")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              1
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("2")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              2
            </Button>
            <Button
              variant="outline"
              onClick={() => handleDigit("3")}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              3
            </Button>
            <Button
              variant="default"
              onClick={() => handleOperator("+")}
              className="h-14 font-semibold text-xl rounded-xl transition-all duration-200 active:scale-95 bg-primary/90 text-primary-foreground hover:bg-primary"
            >
              +
            </Button>

            {/* Row 5 */}
            <Button
              variant="outline"
              onClick={() => handleDigit("0")}
              className="h-14 col-span-2 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              0
            </Button>
            <Button
              variant="outline"
              onClick={handleDecimal}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 border-border/50 hover:bg-muted font-mono"
            >
              .
            </Button>
            <Button
              variant="secondary"
              onClick={handleBackspace}
              className="h-14 font-semibold text-lg rounded-xl transition-all duration-200 active:scale-95 bg-secondary/80 hover:bg-secondary text-foreground/80 flex items-center justify-center"
            >
              <Delete className="size-5" />
            </Button>

            {/* Equal Button */}
            <Button
              variant="default"
              onClick={handleEqual}
              className="h-14 col-span-4 font-bold text-xl rounded-xl transition-all duration-200 active:scale-95 bg-indigo-600 hover:bg-indigo-700 text-white shadow-lg shadow-indigo-500/20 dark:shadow-indigo-950/40"
            >
              =
            </Button>
          </div>
        </CardContent>

        {/* History Slide-out Panel */}
        <div
          className={`absolute top-0 right-0 w-80 h-full bg-card border-l border-border/90 shadow-2xl transition-all duration-300 ease-in-out z-10 flex flex-col ${
            showHistory ? "translate-x-0" : "translate-x-full"
          }`}
        >
          <div className="flex justify-between items-center p-4 border-b border-border/70">
            <span className="font-bold text-sm tracking-wide">History</span>
            <div className="flex items-center gap-1">
              {history.length > 0 && (
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={clearHistory}
                  className="size-8 rounded-full text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                >
                  <Trash2 className="size-4" />
                </Button>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowHistory(false)}
                className="text-xs"
              >
                Close
              </Button>
            </div>
          </div>

          <ScrollArea className="flex-1 p-4">
            {history.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-48 text-muted-foreground text-xs gap-1">
                <History className="size-8 opacity-30 mb-1" />
                No calculations yet
              </div>
            ) : (
              <div className="space-y-3">
                {history.map((item) => (
                  <div
                    key={item.id}
                    onClick={() => reuseHistoryItem(item)}
                    className="p-3 rounded-lg border border-border/50 bg-secondary/30 hover:bg-secondary/70 cursor-pointer transition-all duration-200 text-right group font-mono"
                  >
                    <div className="text-xs text-muted-foreground truncate group-hover:text-foreground/80 transition-colors">
                      {item.formula}
                    </div>
                    <div className="text-sm font-semibold mt-1 text-foreground">
                      {item.result}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </ScrollArea>
        </div>
      </Card>
    </div>
  );
}
