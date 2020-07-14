def main() {
    var res = calculate(2, 5)
    print(res)
}

def calculate(a: float, b: float): float{
    var res = a + b
    return res ** a
}