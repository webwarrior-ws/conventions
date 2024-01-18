let cp = require("child_process");

function runCommitLintOnMsg2(inputMsg: string) {
    return cp.spawnSync("npx", ["commitlint", "--verbose"], {
        input: inputMsg,
    });
}

test("body-leading-blank1", () => {
    let commitMsgWithoutEmptySecondLine =
        "foo: this is only a title" + "\n" + "Bar baz.";
    let bodyLeadingBlank1 = runCommitLintOnMsg2(commitMsgWithoutEmptySecondLine);
    expect(bodyLeadingBlank1.status).not.toBe(0);
});

test("subject-full-stop1", () => {
    let commitMsgWithEndingDotInTitle = "foo/bar: bla bla blah.";
    let subjectFullStop1 = runCommitLintOnMsg2(commitMsgWithEndingDotInTitle);
    expect(subjectFullStop1.status).not.toBe(0);
});

test("subject-full-stop2", () => {
    let commitMsgWithoutEndingDotInTitle = "foo/bar: bla bla blah";
    let subjectFullStop2 = runCommitLintOnMsg2(commitMsgWithoutEndingDotInTitle);
    expect(subjectFullStop2.status).toBe(0);
});
