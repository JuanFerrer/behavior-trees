#ifndef BT_BEHAVIOR_TREE_BUILDER_H
#define BT_BEHAVIOR_TREE_BUILDER_H

#include "General.h"
#include <stack>
#include "Branch.h"
#include <functional>


namespace fluentBehaviorTree
{
	class Root;

	class BehaviorTreeBuilder
	{
	private:
		std::string mName;
		std::stack<Branch*> mParentNodes;
		
	protected:
		void setName(std::string name) { mName = name; }

	public:
		std::string getName() { return mName; }
		Node* getRoot()
		{
			return mParentNodes.top();
		}

		BehaviorTreeBuilder(std::string name);
		~BehaviorTreeBuilder() {}

		/*********************/
		/******* LEAF ********/
		/*********************/

		//BehaviorTreeBuilder Do(std::string name, EStatus(*f)());
		BehaviorTreeBuilder Do(std::string name, std::function<EStatus()> f);

		BehaviorTreeBuilder Do(std::string name, Node *tree);

		//BehaviorTreeBuilder If(std::string name, bool(*f)());
		BehaviorTreeBuilder If(std::string name, std::function<bool()> f);


		/*********************/
		/***** DECORATOR *****/
		/*********************/

		BehaviorTreeBuilder Not(std::string name);

		BehaviorTreeBuilder Repeat(std::string name, int n = 0);

		BehaviorTreeBuilder RepeatUntilFail(std::string name);

		BehaviorTreeBuilder Ignore(std::string name);

		BehaviorTreeBuilder Wait(std::string name, size_t ms);

		/*********************/
		/***** COMPOSITE *****/
		/*********************/

		BehaviorTreeBuilder Selector(std::string name);

		BehaviorTreeBuilder Sequence(std::string name);

		BehaviorTreeBuilder End();

	};
}

#endif

