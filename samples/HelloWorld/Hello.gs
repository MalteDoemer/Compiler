def main() {
    var res = calculate(2, 8)
    print(res)
    input()
}

def calculate(a: float, b: float): float{
    var res = a + b
    return res ** a
}